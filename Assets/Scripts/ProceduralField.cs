using ClipperLib;
using Poly2Tri;
using Poly2Tri.Triangulation.Polygon;
using Poly2Tri.Utility;
using QuickType;
using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class ProceduralField : MonoBehaviour
{
    [SerializeField]
    private float homePlateWidth = 7.9248f;
    [SerializeField]
    private float pitchingMoundWidth = 5.4864f;
    [SerializeField]
    private float basePitchMoundDistance = 18.4404f;
    private float pitchMoundDistance = 18.4404f;
    [SerializeField]
    private float runLaneWidth = 2.0f;
    [SerializeField]
    private float baseFowlAngle = 90.0f;
    private float fowlAngle = 90.0f;
    [SerializeField]
    private float baserunLength = 27.432f;
    private float runLength = 27.432f;
    [SerializeField]
    private float baseSideFieldLength = 100;
    private float sideFieldLength = 100;
    [SerializeField]
    private float baseCenterFieldLength = 128;
    private float centerFieldLength = 128;
    [SerializeField]
    private float backStopDistance = 18.288f;
    [SerializeField]
    private float warningZoneWidth = 5.0f;

    [SerializeField]
    private float baseWallHeight = 4;
    private float wallHeight = 4;

    private float fieldSlope = 0;

    [SerializeField]
    private int quarterPlateVerts = 5;

    [SerializeField]
    private Transform homeBaseObj;
    [SerializeField]
    private Transform firstBaseObj;
    [SerializeField]
    private Transform secondBaseObj;
    [SerializeField]
    private Transform thirdBaseObj;
    [SerializeField]
    private Transform fourthBaseObj;
    [SerializeField]
    private Transform pitchingRubberObj;

    [Range(0, 1)]
    public float grandiosity = 0.5f;
    [Range(0, 1)]
    public float fortification = 0.5f;
    [Range(0, 1)]
    public float obtuseness = 0.5f;

    public void LoadStadium(StadiumData stadium)
    {
        grandiosity = stadium.Grandiosity;
        fortification = stadium.Fortification;
        obtuseness = stadium.Obtuseness;
        ominousness = stadium.Ominousness;
        inconvenience = stadium.Inconvenience;
        viscosity = stadium.Viscosity;
        forwardness = stadium.Forwardness;
        mysticism = stadium.Mysticism;
        elongation = stadium.Elongation;
        filthiness = stadium.Filthiness;
        luxuriousness = stadium.Luxuriousness;
        hype = stadium.Hype;
        hasGrindRail = stadium.Mods.Contains("GRIND_RAIL");

        GenerateField();
    }

    [Range(0, 1)]
    public float ominousness = 0.5f;
    [Range(0, 1)]
    public float inconvenience = 0.5f;
    [Range(0, 1)]
    public float viscosity = 0.5f;
    [Range(0, 1)]
    public float forwardness = 0.5f;
    [Range(0, 1)]
    public float mysticism = 0.5f;
    [Range(0, 1)]
    public float elongation = 0.5f;
    [Range(0, 1)]
    public float filthiness = 0.0f;
    [Range(0, 1)]
    public float luxuriousness = 0.0f;
    [Range(0, 1)]
    public float hype = 0.0f;

    [SerializeField]
    public bool hasFourthBase = false;

    [SerializeField]
    public bool hasGrindRail = false;

    private Vector3 pitchingMoundCenter;

    private Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        GenerateField();
    }

    private void OnValidate()
    {
        GenerateField();
    }

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void AdjustMeasurements()
    {
        float ob = obtuseness;
        ob -= 0.5f;
        ob *= 1.65f;
        ob += 1;
        fowlAngle = baseFowlAngle * ob;
        float lon = elongation;
        lon -= 0.5f;
        lon *= 1.5f;
        lon += 1;
        runLength = baserunLength * lon;
        float forward = forwardness;
        forward -= 0.5f;
        forward *= -1.8f;
        forward += 1;
        pitchMoundDistance = basePitchMoundDistance * forward;

        float grand = grandiosity;
        grand -= 0.5f;
        grand *= 1.2f;
        grand += 1;
        sideFieldLength = baseSideFieldLength * grand;
        centerFieldLength = baseCenterFieldLength * grand;

        float omin = ominousness;
        omin -= 0.5f;
        omin *= 2f;
        fieldSlope = omin;

        float fort = fortification;
        fort -= 0.5f;
        fort *= 1.9f;
        fort += 1;
        wallHeight = baseWallHeight * fort;
    }


    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> dirtTriangles = new List<int>();
    List<int> inFieldGrassTriangles = new List<int>();
    List<int> outFieldTriangles = new List<int>();
    List<int> wallTriangles = new List<int>();
    //we use IntPoints because they're better for comparison. If needed we can make a new container.
    Dictionary<int, Dictionary<Vector3Int, int>> pointLookup = new Dictionary<int, Dictionary<Vector3Int, int>>();

    private void GenerateField()
    {
        AdjustMeasurements();

        if (mesh == null)
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
        }
        vertices.Clear();
        uvs.Clear();
        dirtTriangles.Clear();
        inFieldGrassTriangles.Clear();
        outFieldTriangles.Clear();
        wallTriangles.Clear();
        pointLookup.Clear();
        mesh.Clear();

        Vector3 homeBase = new Vector3(0, 0, 0);
        Vector3 firstBase = Quaternion.Euler(0, fowlAngle / 2, 0) * (Vector3.forward * runLength);
        Vector3 thirdBase = Quaternion.Euler(0, fowlAngle / -2, 0) * (Vector3.forward * runLength);
        Vector3 secondBase = firstBase + thirdBase;

        List<IntPoint> actualDiamond = new List<IntPoint>();
        actualDiamond.Add((IntPoint)homeBase);
        actualDiamond.Add((IntPoint)thirdBase);
        actualDiamond.Add((IntPoint)secondBase);
        actualDiamond.Add((IntPoint)firstBase);

        ClipperOffset co = new ClipperOffset();
        co.ArcTolerance = 0.05 * IntPoint.Multiplier;
        co.AddPath(actualDiamond, JoinType.jtRound, EndType.etClosedPolygon);

        if (hasFourthBase)
        {
            co.AddPath(new List<IntPoint> { (IntPoint)homeBase, (IntPoint)(thirdBase * 2) }, JoinType.jtRound, EndType.etOpenRound);
        }

        List<List<IntPoint>> outerBounds = new List<List<IntPoint>>();
        co.Execute(ref outerBounds, runLaneWidth / 2 * IntPoint.Multiplier);

        float baseLaneAngle = Mathf.Asin(runLaneWidth / homePlateWidth) * Mathf.Rad2Deg;

        List<IntPoint> outerBorder = new List<IntPoint>();
        List<IntPoint> insideGrassBorder = new List<IntPoint>();

        Vector3 pitchingRubber = Vector3.forward * pitchMoundDistance;

        //Find some outfield points now so we can make sure the infield doesn't reach past the outfield.
        List<IntPoint> outfieldBorder = new List<IntPoint>();
        float leftOutFieldDistance = Mathf.Max(sideFieldLength, runLength + runLaneWidth / 2);
        if (hasFourthBase)
            leftOutFieldDistance = Mathf.Max(leftOutFieldDistance, runLength * 2 + runLaneWidth / 2);
        var leftOutfieldPoint = Quaternion.Euler(0, -fowlAngle / 2, 0) * (Vector3.forward * leftOutFieldDistance + Vector3.left * runLaneWidth / 2);
        var rightOutfieldPoint = Quaternion.Euler(0, fowlAngle / 2, 0) * (Vector3.forward * Mathf.Max(sideFieldLength, runLength + runLaneWidth / 2) + Vector3.right * runLaneWidth / 2);
        var centerOutfieldPoint = Vector3.forward * Mathf.Max(centerFieldLength, secondBase.magnitude + runLaneWidth / 2);

        var leftInFieldPoint = Quaternion.Euler(0, -fowlAngle / 2, 0) * (Vector3.forward * runLength * 1.4f + Vector3.left * runLaneWidth / 2);
        var rightInFieldPoint = Quaternion.Euler(0, fowlAngle / 2, 0) * (Vector3.forward * runLength * 1.4f + Vector3.right * runLaneWidth / 2);
        var centerInFieldPoint = secondBase + Vector3.forward * runLength * 0.3f;

        float outfieldLineRadius = Mathf.Max(runLength + 5 * 0.3048f, 95 * 0.3048f);

        AddArc(outerBorder, (fowlAngle / 2) + baseLaneAngle, (fowlAngle / -2) - baseLaneAngle + 360, homePlateWidth / 2, new Vector3(0, 0, 0), out Vector3 firstRunA, out Vector3 fourthRunD, 3);

        CircleCenter(leftInFieldPoint, centerInFieldPoint, rightInFieldPoint, out Vector3 threePointCenter, out float threePointRadius);

        AddArcThreePoint(outerBorder, leftInFieldPoint, centerInFieldPoint, rightInFieldPoint, 2);

        //0.4 * run length
        //0.21 * home to second
        //or 0.3 * run length

        AddArc(insideGrassBorder, (fowlAngle / -2) + baseLaneAngle, (fowlAngle / 2) - baseLaneAngle, homePlateWidth / 2, homeBase);
        AddArc(insideGrassBorder, (fowlAngle / 2) + baseLaneAngle + 180, (fowlAngle / -2) - baseLaneAngle + 360, homePlateWidth / 2, firstBase);
        AddArc(insideGrassBorder, 180 - (fowlAngle / 2) + baseLaneAngle, 180 + (fowlAngle / 2) - baseLaneAngle, homePlateWidth / 2, secondBase);
        AddArc(insideGrassBorder, (fowlAngle / 2) + baseLaneAngle, (fowlAngle / -2) - baseLaneAngle + 180, homePlateWidth / 2, thirdBase);

        //pitching mound
        pitchingMoundCenter = Vector3.forward * (pitchMoundDistance - 18 * 0.0254f);
        List<IntPoint> pitchingMoundBorder = new List<IntPoint>();
        for (int i = 0; i < quarterPlateVerts * 4; i++)
        {
            pitchingMoundBorder.Add((IntPoint)(pitchingMoundCenter + Quaternion.Euler(0, 360.0f / (quarterPlateVerts * 4) * i, 0) * (Vector3.forward * pitchingMoundWidth / 2)));
        }

        Clipper innerGrassClipper = new Clipper();
        innerGrassClipper.AddPath(insideGrassBorder, PolyType.ptSubject, true);
        innerGrassClipper.AddPath(pitchingMoundBorder, PolyType.ptClip, true);

        PolyTree grassTree = new PolyTree();
        innerGrassClipper.Execute(ClipType.ctDifference, grassTree, PolyFillType.pftPositive, PolyFillType.pftNegative);

        AddPolyTree(inFieldGrassTriangles, grassTree);

        Clipper initialDirt = new Clipper();
        initialDirt.AddPath(outerBorder, PolyType.ptSubject, true);
        initialDirt.AddPath(pitchingMoundBorder, PolyType.ptClip, true);
        initialDirt.AddPath(outerBounds[0], PolyType.ptClip, true);

        List<List<IntPoint>> joinedDirt = new List<List<IntPoint>>();
        initialDirt.Execute(ClipType.ctUnion, joinedDirt, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

        Clipper dirtClipper = new Clipper();
        dirtClipper.AddPaths(joinedDirt, PolyType.ptSubject, true);
        //this is just temporary. Won't be needed later.
        dirtClipper.AddPaths(Clipper.ClosedPathsFromPolyTree(grassTree), PolyType.ptClip, true);


        PolyTree dirtTree = new PolyTree();
        dirtClipper.Execute(ClipType.ctDifference, dirtTree, PolyFillType.pftNonZero, PolyFillType.pftPositive);

        AddPolyTree(dirtTriangles, dirtTree);

        //Back Field
        List<IntPoint> backField = new List<IntPoint>();
        AddArc(backField, fowlAngle / -2 + 270, fowlAngle / 2 + 90, backStopDistance, homeBase);
        backField.Add((IntPoint)(firstBase + (Quaternion.Euler(0, fowlAngle / 2 + 90, 0) * (Vector3.forward * backStopDistance))));
        backField.Add((IntPoint)((firstBase + rightOutfieldPoint) / 2));
        backField.Add((IntPoint)homeBase);
        if (hasFourthBase)
            backField.Add((IntPoint)((thirdBase + thirdBase + leftOutfieldPoint) / 2));
        else
            backField.Add((IntPoint)((thirdBase + leftOutfieldPoint) / 2));
        backField.Add((IntPoint)(thirdBase + (Quaternion.Euler(0, fowlAngle / -2 - 90, 0) * (Vector3.forward * backStopDistance))));

        Clipper rearFieldClipper = new Clipper();
        rearFieldClipper.AddPath(backField, PolyType.ptSubject, true);
        rearFieldClipper.AddPaths(Clipper.ClosedPathsFromPolyTree(dirtTree), PolyType.ptClip, true);
        rearFieldClipper.AddPaths(Clipper.ClosedPathsFromPolyTree(grassTree), PolyType.ptClip, true);

        PolyTree rearFieldTree = new PolyTree();
        rearFieldClipper.Execute(ClipType.ctDifference, rearFieldTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

        //construct the actual outfield.
        AddArcThreePoint(outfieldBorder, leftOutfieldPoint, centerOutfieldPoint, rightOutfieldPoint, 4);

        outfieldBorder.Add((IntPoint)(Quaternion.Euler(0, fowlAngle / 2, 0) * (Vector3.forward * runLength + Vector3.right * warningZoneWidth / 2)));
        outfieldBorder.Add((IntPoint)(Quaternion.Euler(0, -fowlAngle / 2, 0) * (Vector3.forward * runLength + Vector3.left * warningZoneWidth / 2)));

        Clipper outfieldClipper = new Clipper();
        outfieldClipper.AddPath(outfieldBorder, PolyType.ptSubject, true);
        outfieldClipper.AddPaths(Clipper.ClosedPathsFromPolyTree(dirtTree), PolyType.ptClip, true);
        outfieldClipper.AddPaths(Clipper.ClosedPathsFromPolyTree(grassTree), PolyType.ptClip, true);
        outfieldClipper.AddPaths(Clipper.ClosedPathsFromPolyTree(rearFieldTree), PolyType.ptClip, true);

        PolyTree outFieldTree = new PolyTree();
        outfieldClipper.Execute(ClipType.ctDifference, outFieldTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

        //Warning Strip
        Clipper warningClipper = new Clipper();
        warningClipper.AddPaths(Clipper.ClosedPathsFromPolyTree(dirtTree), PolyType.ptSubject, true);
        warningClipper.AddPaths(Clipper.ClosedPathsFromPolyTree(grassTree), PolyType.ptSubject, true);
        warningClipper.AddPaths(Clipper.ClosedPathsFromPolyTree(outFieldTree), PolyType.ptSubject, true);
        warningClipper.AddPaths(Clipper.ClosedPathsFromPolyTree(rearFieldTree), PolyType.ptSubject, true);

        List<List<IntPoint>> border = new List<List<IntPoint>>();
        warningClipper.Execute(ClipType.ctUnion, border);

        border = Clipper.CleanPolygons(border, 0.1 * IntPoint.Multiplier);

        ClipperOffset warningOffset = new ClipperOffset();
        warningOffset.AddPaths(border, JoinType.jtMiter, EndType.etClosedPolygon);

        List<List<IntPoint>> warningInsideBorder = new List<List<IntPoint>>();
        warningOffset.Execute(ref warningInsideBorder, -warningZoneWidth*IntPoint.Multiplier);

        warningClipper.AddPaths(warningInsideBorder, PolyType.ptClip, true);

        PolyTree warningAreaTree = new PolyTree();
        warningClipper.Execute(ClipType.ctDifference, warningAreaTree, PolyFillType.pftNonZero, PolyFillType.pftPositive);

        AddPolyTree(dirtTriangles, warningAreaTree);

        //remove warning area from outfield
        outfieldClipper.AddPaths(Clipper.ClosedPathsFromPolyTree(warningAreaTree), PolyType.ptClip, true);
        outfieldClipper.Execute(ClipType.ctDifference, outFieldTree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
        AddPolyTree(outFieldTriangles, outFieldTree);

        //remove warning area from rear field
        rearFieldClipper.AddPaths(Clipper.ClosedPathsFromPolyTree(warningAreaTree), PolyType.ptClip, true);
        rearFieldClipper.Execute(ClipType.ctDifference, rearFieldTree);
        AddPolyTree(inFieldGrassTriangles, rearFieldTree);


        //Wall
        CreateWall(wallTriangles, border[0], wallHeight, 0.25f, EndType.etClosedLine);

        //Grind raild
        if(hasGrindRail)
        {
            List<IntPoint> grindRail = new List<IntPoint>();
            AddArcThreePoint(grindRail, thirdBase + Vector3.right * runLaneWidth, pitchingRubber + Vector3.forward * pitchingMoundWidth / 2, firstBase + Vector3.left * runLaneWidth, 3);
            CreateWall(wallTriangles, grindRail, 0.2f, 0.05f);
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = AddSlope(vertices[i]);
        }

        mesh.subMeshCount = 4;
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.SetTriangles(dirtTriangles.ToArray(), 0, false);
        mesh.SetTriangles(inFieldGrassTriangles.ToArray(), 1, false);
        mesh.SetTriangles(outFieldTriangles, 2, false);
        mesh.SetTriangles(wallTriangles, 3, false);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        if (homeBaseObj != null)
            homeBaseObj.position = transform.TransformPoint(AddSlope(homeBase));
        if (firstBaseObj != null)
            firstBaseObj.position = transform.TransformPoint(AddSlope(firstBase));
        if (secondBaseObj != null)
            secondBaseObj.position = transform.TransformPoint(AddSlope(secondBase));
        if (thirdBaseObj != null)
            thirdBaseObj.position = transform.TransformPoint(AddSlope(thirdBase));
        if (fourthBaseObj != null)
        {
            if (hasFourthBase)
            {
                fourthBaseObj.position = transform.TransformPoint(AddSlope(thirdBase * 2));
                fourthBaseObj.gameObject.SetActive(true);
            }
            else
                fourthBaseObj.gameObject.SetActive(false);
        }
        if (pitchingRubberObj != null)
            pitchingRubberObj.position = transform.TransformPoint(AddSlope(pitchingRubber));

    }
    [SerializeField]
    private float slopeA = 5;
    [SerializeField]
    private float slopeB = 5;
    private Vector3 AddSlope(Vector3 p)
    {
        float distance = (p - pitchingMoundCenter).magnitude;
        p.y += ((slopeA * Mathf.Sqrt(slopeB * slopeB + distance * distance) / slopeB) - slopeA) * fieldSlope;
        //Debug.Log(multiplier);
        return p;
    }

    private void AddPoly(List<int> outputTriangles, List<IntPoint> border, float height = 0)
    {
        List<PolygonPoint> points = new List<PolygonPoint>();
        foreach (var point in border)
        {
            points.Add((PolygonPoint)point);
        }
        var poly = new Polygon(points);
        P2T.Triangulate(poly);

        foreach (var tri in poly.Triangles)
        {
            outputTriangles.Add(AddVert(new Vector3(tri.Points[0].Xf, height, tri.Points[0].Yf)));
            outputTriangles.Add(AddVert(new Vector3(tri.Points[2].Xf, height, tri.Points[2].Yf)));
            outputTriangles.Add(AddVert(new Vector3(tri.Points[1].Xf, height, tri.Points[1].Yf)));
        }
    }

    private void AddPolyTree(List<int> outputTriangles, PolyTree tree, float height = 0, int smoothingGroup = 0)
    {
        foreach (var branch in tree.Childs)
        {
            AddPolyNode(outputTriangles, branch, height, smoothingGroup);
        }
    }

    private void AddPolyNode(List<int> outputTriangles, PolyNode branch, float height = 0, int smoothingGroup = 0)
    {
        //This should only be called on external polys.
        if (branch.IsHole || branch.IsOpen)
            return;

        var cleaned = Clipper.CleanPolygon(branch.Contour, 0.001 * IntPoint.Multiplier);
        //if it's empty, we can't actually do anything with it.
        if (cleaned.Count < 3)
            return;
        List<PolygonPoint> points = new List<PolygonPoint>();
        foreach (var point in cleaned)
        {
            points.Add((PolygonPoint)point);
        }
        var poly = new Polygon(points);

        foreach (var child in branch.Childs)
        {
            if (child.IsHole)
            {
                var cleanedHole = Clipper.CleanPolygon(child.Contour, 0.001 * IntPoint.Multiplier);
                if (cleanedHole.Count < 3)
                    continue;
                List<PolygonPoint> holePoints = new List<PolygonPoint>();
                foreach (var holePoint in cleanedHole)
                {
                    holePoints.Add((PolygonPoint)holePoint);
                }
                poly.AddHole(new Polygon(holePoints));
                foreach (var grandChild in child.Childs)
                {
                    AddPolyNode(outputTriangles, grandChild);
                }
            }
            else
                AddPolyNode(outputTriangles, child);
        }
        P2T.Triangulate(poly);

        foreach (var tri in poly.Triangles)
        {
            AddTriangle(outputTriangles, tri, height, smoothingGroup);
        }
    }

    private void AddTriangle(List<int> outputTriangles, Poly2Tri.Triangulation.Delaunay.DelaunayTriangle tri, float height = 0, int smoothingGroup = 0)
    {
        AddSubdividedTriangleX9(outputTriangles, tri, height, smoothingGroup);

        //outputTriangles.Add(AddVert((Vector3)tri.Points[0]));
        //outputTriangles.Add(AddVert((Vector3)tri.Points[2]));
        //outputTriangles.Add(AddVert((Vector3)tri.Points[1]));

    }

    private void AddSubdividedTriangleX4(List<int> outputTriangles, Poly2Tri.Triangulation.Delaunay.DelaunayTriangle tri)
    {
        var p1 = tri.Points[0];
        var p2 = tri.Points[1];
        var p3 = tri.Points[2];

        var m1 = (p1 + p2) / 2;
        var m2 = (p2 + p3) / 2;
        var m3 = (p3 + p1) / 2;

        outputTriangles.Add(AddVert((Vector3)p1));
        outputTriangles.Add(AddVert((Vector3)m3));
        outputTriangles.Add(AddVert((Vector3)m1));

        outputTriangles.Add(AddVert((Vector3)p2));
        outputTriangles.Add(AddVert((Vector3)m1));
        outputTriangles.Add(AddVert((Vector3)m2));

        outputTriangles.Add(AddVert((Vector3)p3));
        outputTriangles.Add(AddVert((Vector3)m2));
        outputTriangles.Add(AddVert((Vector3)m3));

        outputTriangles.Add(AddVert((Vector3)m1));
        outputTriangles.Add(AddVert((Vector3)m3));
        outputTriangles.Add(AddVert((Vector3)m2));

    }

    private void AddSubdividedTriangleX9(List<int> outputTriangles, Poly2Tri.Triangulation.Delaunay.DelaunayTriangle tri, float height = 0, int smoothingGroup = 0)
    {
        var i1 = tri.Points[0];
        var i2 = tri.Points[2];
        var i3 = tri.Points[1];

        var a1 = GetNewVertex9(i1, i2, i1);
        var a2 = GetNewVertex9(i2, i1, i2);
        var b1 = GetNewVertex9(i2, i3, i2);
        var b2 = GetNewVertex9(i3, i2, i3);
        var c1 = GetNewVertex9(i3, i1, i3);
        var c2 = GetNewVertex9(i1, i3, i1);

        var d = GetNewVertex9(i1, i2, i3);

        outputTriangles.Add(AddVert(ConvertPoint(i1, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(a1, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(c2, height), smoothingGroup));
        outputTriangles.Add(AddVert(ConvertPoint(i2, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(b1, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(a2, height), smoothingGroup));
        outputTriangles.Add(AddVert(ConvertPoint(i3, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(c1, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(b2, height), smoothingGroup));
        outputTriangles.Add(AddVert(ConvertPoint(d, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(a1, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(a2, height), smoothingGroup));
        outputTriangles.Add(AddVert(ConvertPoint(d, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(b1, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(b2, height), smoothingGroup));
        outputTriangles.Add(AddVert(ConvertPoint(d, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(c1, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(c2, height), smoothingGroup));
        outputTriangles.Add(AddVert(ConvertPoint(d, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(c2, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(a1, height), smoothingGroup));
        outputTriangles.Add(AddVert(ConvertPoint(d, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(a2, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(b1, height), smoothingGroup));
        outputTriangles.Add(AddVert(ConvertPoint(d, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(b2, height), smoothingGroup)); outputTriangles.Add(AddVert(ConvertPoint(c1, height), smoothingGroup));
    }

    private void AddSubdividedTriangleX9(List<int> outputTriangles, Vector3 i1, Vector3 i2, Vector3 i3, int smoothingGroup = 0)
    {
        var a1 = GetNewVertex9(i1, i2, i1);
        var a2 = GetNewVertex9(i2, i1, i2);
        var b1 = GetNewVertex9(i2, i3, i2);
        var b2 = GetNewVertex9(i3, i2, i3);
        var c1 = GetNewVertex9(i3, i1, i3);
        var c2 = GetNewVertex9(i1, i3, i1);

        var d = GetNewVertex9(i1, i2, i3);

        outputTriangles.Add(AddVert(i1, smoothingGroup)); outputTriangles.Add(AddVert(a1, smoothingGroup)); outputTriangles.Add(AddVert(c2, smoothingGroup));
        outputTriangles.Add(AddVert(i2, smoothingGroup)); outputTriangles.Add(AddVert(b1, smoothingGroup)); outputTriangles.Add(AddVert(a2, smoothingGroup));
        outputTriangles.Add(AddVert(i3, smoothingGroup)); outputTriangles.Add(AddVert(c1, smoothingGroup)); outputTriangles.Add(AddVert(b2, smoothingGroup));
        outputTriangles.Add(AddVert(d, smoothingGroup)); outputTriangles.Add(AddVert(a1, smoothingGroup)); outputTriangles.Add(AddVert(a2, smoothingGroup));
        outputTriangles.Add(AddVert(d, smoothingGroup)); outputTriangles.Add(AddVert(b1, smoothingGroup)); outputTriangles.Add(AddVert(b2, smoothingGroup));
        outputTriangles.Add(AddVert(d, smoothingGroup)); outputTriangles.Add(AddVert(c1, smoothingGroup)); outputTriangles.Add(AddVert(c2, smoothingGroup));
        outputTriangles.Add(AddVert(d, smoothingGroup)); outputTriangles.Add(AddVert(c2, smoothingGroup)); outputTriangles.Add(AddVert(a1, smoothingGroup));
        outputTriangles.Add(AddVert(d, smoothingGroup)); outputTriangles.Add(AddVert(a2, smoothingGroup)); outputTriangles.Add(AddVert(b1, smoothingGroup));
        outputTriangles.Add(AddVert(d, smoothingGroup)); outputTriangles.Add(AddVert(b2, smoothingGroup)); outputTriangles.Add(AddVert(c1, smoothingGroup));
    }

    private Vector3 ConvertPoint(Poly2Tri.Triangulation.TriangulationPoint point, float height = 0)
    {
        return new Vector3(point.Xf, height, point.Yf);
    }
    private Vector3 ConvertPoint(Point2D point, float height = 0)
    {
        return new Vector3(point.Xf, height, point.Yf);
    }

    Point2D GetNewVertex9(Point2D a, Point2D b, Point2D c)
    {
        return (a + b + c) / 3;
    }
    Vector3 GetNewVertex9(Vector3 a, Vector3 b, Vector3 c)
    {
        return (a + b + c) / 3;
    }

    private void CreateWall(List<int> triangleList, List<IntPoint> line, float height, float thickness, EndType endType = EndType.etOpenSquare, float smoothAngle = 20)
    {
        ClipperOffset co = new ClipperOffset();
        co.ArcTolerance = 0.05 * IntPoint.Multiplier;
        co.AddPath(line, JoinType.jtMiter, endType);
        PolyTree wallPolys = new PolyTree();
        co.Execute(ref wallPolys, thickness * IntPoint.Multiplier);
        AddPolyTree(triangleList, wallPolys, height, 1);

        var polys = Clipper.ClosedPathsFromPolyTree(wallPolys);
        foreach (var poly in polys)
        {
            Vector3 startTopPoint = Vector3.zero;
            Vector3 startBottomPoint = Vector3.zero;
            Vector3 lastTopPoint = Vector3.zero;
            Vector3 lastBottomPoint = Vector3.zero;
            int currentSmoothGroup = 2;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector3 bottomPoint = (Vector3)poly[i];
                Vector3 topPoint = bottomPoint + new Vector3(0, height, 0);
                if(i > 0 && i < poly.Count-1)
                {
                    if ((180 -Vector3.Angle((Vector3)poly[i + 1] - bottomPoint, lastBottomPoint - bottomPoint)) > smoothAngle)
                        currentSmoothGroup++;
                }
                else if (i == poly.Count)
                {
                    if ((180 - Vector3.Angle(startBottomPoint - bottomPoint, lastBottomPoint - bottomPoint)) > smoothAngle)
                        currentSmoothGroup++;
                }
                if (i == 0)
                {
                    startTopPoint = topPoint;
                    startBottomPoint = bottomPoint;
                }
                else
                {
                    AddSubdividedTriangleX9(triangleList, lastBottomPoint, lastTopPoint, topPoint, currentSmoothGroup);
                    AddSubdividedTriangleX9(triangleList, lastBottomPoint, topPoint, bottomPoint, currentSmoothGroup);
                }
                lastTopPoint = topPoint;
                lastBottomPoint = bottomPoint;
            }
            if ((180 - Vector3.Angle((Vector3)poly[1] - startBottomPoint, lastBottomPoint - startBottomPoint)) > smoothAngle)
                currentSmoothGroup++;
            AddSubdividedTriangleX9(triangleList, lastBottomPoint, lastTopPoint, startTopPoint, currentSmoothGroup);
            AddSubdividedTriangleX9(triangleList, lastBottomPoint, startTopPoint, startBottomPoint, currentSmoothGroup);
        }
    }


    private int AddVert(Vector3 pos, int smoothingGroup = 0)
    {
        if (!pointLookup.ContainsKey(smoothingGroup))
            pointLookup[smoothingGroup] = new Dictionary<Vector3Int, int>();
        Vector3Int ip = Vector3Int.FloorToInt(pos * 1000);
        if (pointLookup[smoothingGroup].ContainsKey(ip))
            return pointLookup[smoothingGroup][ip];
        int index = vertices.Count;
        vertices.Add(pos);
        uvs.Add(new Vector2(pos.x, pos.z));
        pointLookup[smoothingGroup][ip] = index;
        return index;
    }

    private void AddArc(List<IntPoint> points, float startAngle, float endAngle, float radius, Vector3 centerPoint, int segmentMultiplier = 1)
    {
        AddArc(points, startAngle, endAngle, radius, centerPoint, out Vector3 dummy1, out Vector3 dummy2, segmentMultiplier);
    }

    private void AddArc(List<IntPoint> points, float startAngle, float endAngle, float radius, Vector3 centerPoint, out Vector3 startVertex, out Vector3 endVertex, int segmentMultiplier = 1)
    {
        int verts = quarterPlateVerts * segmentMultiplier;
        float angleStep = (endAngle - startAngle) / verts;
        startVertex = Quaternion.Euler(0, startAngle, 0) * (Vector3.forward * radius) + centerPoint;
        endVertex = Quaternion.Euler(0, endAngle, 0) * (Vector3.forward * radius) + centerPoint;
        for (int i = 0; i <= verts; i++)
        {
            points.Add((IntPoint)(Quaternion.Euler(0, startAngle + i * angleStep, 0) * (Vector3.forward * radius) + centerPoint));
        }
    }

    // Find the points of intersection.
    private int FindLineCircleIntersections(Vector3 circleCenter, float radius, Vector3 point1, Vector3 point2, out Vector3 intersection1, out Vector3 intersection2)
    {
        float dx, dy, A, B, C, det, t;

        dx = point2.x - point1.x;
        dy = point2.z - point1.z;

        A = dx * dx + dy * dy;
        B = 2 * (dx * (point1.x - circleCenter.x) + dy * (point1.z - circleCenter.z));
        C = (point1.x - circleCenter.x) * (point1.x - circleCenter.x) +
            (point1.z - circleCenter.z) * (point1.z - circleCenter.z) -
            radius * radius;

        det = B * B - 4 * A * C;
        if ((A <= 0.0000001) || (det < 0))
        {
            // No real solutions.
            intersection1 = new Vector3(float.NaN, 0, float.NaN);
            intersection2 = new Vector3(float.NaN, 0, float.NaN);
            return 0;
        }
        else if (det == 0)
        {
            // One solution.
            t = -B / (2 * A);
            intersection1 =
                new Vector3(point1.x + t * dx, 0, point1.z + t * dy);
            intersection2 = new Vector3(float.NaN, float.NaN);
            return 1;
        }
        else
        {
            // Two solutions.
            t = (float)((-B + Math.Sqrt(det)) / (2 * A));
            intersection1 =
                new Vector3(point1.x + t * dx, 0, point1.z + t * dy);
            t = (float)((-B - Math.Sqrt(det)) / (2 * A));
            intersection2 =
                new Vector3(point1.x + t * dx, 0, point1.z + t * dy);
            return 2;
        }
    }

    private void AddArcThreePoint(List<IntPoint> points, Vector3 A, Vector3 B, Vector3 C, int segmentMultiplier = 1)
    {
        CircleCenter(A, B, C, out Vector3 center, out float radius);
        var andleA = Quaternion.LookRotation(A - center).eulerAngles.y;
        var angleB = Quaternion.LookRotation(C - center).eulerAngles.y;
        var middleDirection = B - center;
        if(middleDirection.z > 0)
        {
            angleB += 360;
        }
        else
        {

        }
        AddArc(points, andleA, angleB, radius, center, segmentMultiplier);
    }

    private void CircleCenter(Vector3 A, Vector3 B, Vector3 C, out Vector3 center, out float radius)
    {

        float yDelta_a = B.z - A.z;
        float xDelta_a = B.x - A.x;
        float yDelta_b = C.z - B.z;
        float xDelta_b = C.x - B.x;
        center = Vector3.zero;

        float aSlope = yDelta_a / xDelta_a;
        float bSlope = yDelta_b / xDelta_b;
        center.x = (aSlope * bSlope * (A.z - C.z) + bSlope * (A.x + B.x)
            - aSlope * (B.x + C.x)) / (2 * (bSlope - aSlope));
        center.z = -1 * (center.x - (A.x + B.x) / 2) / aSlope + (A.z + B.z) / 2;
        radius = (center - A).magnitude;
    }

    public Vector3 FindNearestPointOnLine(Vector3 origin, Vector3 direction, Vector3 point)
    {
        direction.Normalize();
        Vector3 lhs = point - origin;

        float dotP = Vector3.Dot(lhs, direction);
        return origin + direction * dotP;
    }
}
