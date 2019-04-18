using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class MeshCombineUtility {
	
    //public struct MeshInstance
    //{
    //    public Mesh      mesh;
    //    public int       subMeshIndex;            
    //    public Matrix4x4 transform;
    //}

    public static Mesh Combine(string name, CombineInstance[] combines, bool generateStrips,bool simple = true)
	{
		int vertexCount = 0;
		int triangleCount = 0;
		int stripCount = 0;
        foreach (CombineInstance combine in combines)
		{
			if (combine.mesh)
			{
				vertexCount += combine.mesh.vertexCount;
				
				if (generateStrips)
				{
					// SUBOPTIMAL FOR PERFORMANCE
                    int curStripCount = combine.mesh.GetTriangles(combine.subMeshIndex).Length;
					if (curStripCount != 0)
					{
						if( stripCount != 0 )
						{
							if ((stripCount & 1) == 1 )
								stripCount += 3;
							else
								stripCount += 2;
						}
						stripCount += curStripCount;
					}
					else
					{
						generateStrips = false;
					}
				}
			}
		}
		
		// Precomputed how many triangles we need instead
		if (!generateStrips)
		{
            foreach (CombineInstance combine in combines)
			{
				if (combine.mesh)
				{
					triangleCount += combine.mesh.GetTriangles(combine.subMeshIndex).Length;
				}
			}
		}
		
		Vector3[] vertices = new Vector3[vertexCount] ;
		Vector3[] normals = new Vector3[vertexCount] ;
		Vector4[] tangents = new Vector4[vertexCount] ;
		Vector2[] uv = new Vector2[vertexCount];
		Vector2[] uv1 = new Vector2[vertexCount];
		int[] triangles = new int[triangleCount];
		int[] strip = new int[stripCount];
		
		int offset;
		
		offset=0;
        foreach (CombineInstance combine in combines)
		{
			if (combine.mesh)
				Copy(combine.mesh.vertexCount, combine.mesh.vertices, vertices, ref offset, combine.transform);
		}

        offset = 0;
        foreach (CombineInstance combine in combines)
        {
            if (combine.mesh)
                Copy(combine.mesh.vertexCount, combine.mesh.uv, uv, ref offset);
        }
        if (!simple)
        {
            offset = 0;
            foreach (CombineInstance combine in combines)
            {
                if (combine.mesh)
                {
                    Matrix4x4 invTranspose = combine.transform;
                    invTranspose = invTranspose.inverse.transpose;
                    CopyNormal(combine.mesh.vertexCount, combine.mesh.normals, normals, ref offset, invTranspose);
                }

            }
            offset = 0;
            foreach (CombineInstance combine in combines)
            {
                if (combine.mesh)
                {
                    Matrix4x4 invTranspose = combine.transform;
                    invTranspose = invTranspose.inverse.transpose;
                    CopyTangents(combine.mesh.vertexCount, combine.mesh.tangents, tangents, ref offset, invTranspose);
                }

            }

            offset = 0;
            foreach (CombineInstance combine in combines)
            {
                if (combine.mesh)
                    Copy(combine.mesh.vertexCount, combine.mesh.uv2, uv1, ref offset);
            }
        }
		
		int triangleOffset=0;
		int stripOffset=0;
		int vertexOffset=0;
        foreach (CombineInstance combine in combines)
		{
			if (combine.mesh)
			{
				if (generateStrips)
				{
                    int[] inputstrip = combine.mesh.GetTriangles(combine.subMeshIndex);
					if (stripOffset != 0)
					{
						if ((stripOffset & 1) == 1)
						{
							strip[stripOffset+0] = strip[stripOffset-1];
							strip[stripOffset+1] = inputstrip[0] + vertexOffset;
							strip[stripOffset+2] = inputstrip[0] + vertexOffset;
							stripOffset+=3;
						}
						else
						{
							strip[stripOffset+0] = strip[stripOffset-1];
							strip[stripOffset+1] = inputstrip[0] + vertexOffset;
							stripOffset+=2;
						}
					}
					
					for (int i=0;i<inputstrip.Length;i++)
					{
						strip[i+stripOffset] = inputstrip[i] + vertexOffset;
					}
					stripOffset += inputstrip.Length;
				}
				else
				{
					int[]  inputtriangles = combine.mesh.GetTriangles(combine.subMeshIndex);
					for (int i=0;i<inputtriangles.Length;i++)
					{
						triangles[i+triangleOffset] = inputtriangles[i] + vertexOffset;
					}
					triangleOffset += inputtriangles.Length;
				}
				
				vertexOffset += combine.mesh.vertexCount;
			}
		}
		Mesh mesh = new Mesh();
        mesh.name = name;
        mesh.vertices = vertices;
        mesh.uv = uv;
        if(!simple)
        {
            mesh.normals = normals;
            mesh.tangents = tangents;
            mesh.uv2 = uv1;
        }
		if (generateStrips)
            mesh.SetTriangles(strip, 0);
		else
			mesh.triangles = triangles;
		return mesh;
	}
	
	static void Copy (int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = transform.MultiplyPoint(src[i]);
		offset += vertexcount;
	}

	static void CopyNormal (int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = transform.MultiplyVector(src[i]).normalized;
		offset += vertexcount;
	}

	static void Copy (int vertexcount, Vector2[] src, Vector2[] dst, ref int offset)
	{
		for (int i=0;i<src.Length;i++)
			dst[i+offset] = src[i];
		offset += vertexcount;
	}

	static void CopyTangents (int vertexcount, Vector4[] src, Vector4[] dst, ref int offset, Matrix4x4 transform)
	{
		for (int i=0;i<src.Length;i++)
		{
			Vector4 p4 = src[i];
			Vector3 p = new Vector3(p4.x, p4.y, p4.z);
			p = transform.MultiplyVector(p);
			dst[i+offset] = new Vector4(p.x, p.y, p.z, p4.w);
		}
			
		offset += vertexcount;
	}



    public static void CombineSkinnedToMesh(GameObject go)
    {
        SkinnedMeshRenderer[] smr = go.GetComponentsInChildren<SkinnedMeshRenderer>();

        //Struct used to describe meshes to be combined ��mesh, submeshindex, transform��
        List<CombineInstance> lcbi = new List<CombineInstance>();

        List<Material> lm = new List<Material>();
        List<Transform> lt = new List<Transform>();

        for (int i = 0; i < smr.Length; i++)
        {
            lm.AddRange(smr[i].materials);
            lt.AddRange(smr[i].bones);

            for (int sub = 0; sub < smr[i].sharedMesh.subMeshCount; sub++)
            {
                CombineInstance cbi = new CombineInstance();
                cbi.mesh = smr[i].sharedMesh;
                cbi.subMeshIndex = sub;
                lcbi.Add(cbi);
            }
            GameObject.Destroy(smr[i].gameObject);
        }

        SkinnedMeshRenderer _smr = go.GetComponent<SkinnedMeshRenderer>();
        if (_smr == null)
        {
            _smr = go.AddComponent<SkinnedMeshRenderer>();
        }

        _smr.sharedMesh = new Mesh();
        _smr.bones = lt.ToArray();
        _smr.materials = new Material[] { lm[0] };
        _smr.rootBone = go.transform;
        _smr.sharedMesh.CombineMeshes(lcbi.ToArray(), true, false);

    }

    public static void CombineMesh(MeshFilter mFilter,MeshFilter[] mfs)
    {
        Mesh mMesh = new Mesh();
        mMesh.Clear();
        //float startTime = Time.realtimeSinceStartup;
        List<CombineInstance> lcbi = new List<CombineInstance>();

        int uvCount = 0;
        List<Vector2[]> uvList = new List<Vector2[]>();
        List<int> texIdx = new List<int>();
        StringBuilder error = new StringBuilder();
        for (int i = 0; i < mfs.Length; i++)
        {
            MeshFilter mf = mfs[i];
            if (mf == mFilter)
                continue;
            Mesh mesh = mf.sharedMesh;
            if (mesh == null)
                continue;
            Vector3 mfScalue = mf.transform.localScale;
            if (mfScalue.x < 0f || mfScalue.y < 0f || mfScalue.z < 0f)
            {
                error.Append(mf.name + "\n");
            }
            uvList.Add(mesh.uv);
            uvCount += mesh.uv.Length;
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                CombineInstance cbi = new CombineInstance();
                cbi.mesh = mesh;
                cbi.subMeshIndex = sub;
                cbi.transform = mf.transform.localToWorldMatrix;
                lcbi.Add(cbi);
            }
        }
        Vector2[] atlasUVs = new Vector2[uvCount];
        int j = 0;
        for (int i = 0; i < uvList.Count; i++)
        {
            foreach (Vector2 uv in uvList[i])
            {
                atlasUVs[j] = uv;
                j++;
            }
        }
        mMesh = Combine("1", lcbi.ToArray(), false);
#if UNITY_EDITOR
        UnityEditor.MeshUtility.Optimize(mMesh);
        UnityEditor.MeshUtility.SetMeshCompression(mMesh, UnityEditor.ModelImporterMeshCompression.Medium);
#endif
        //mMesh.CombineMeshes(lcbi.ToArray(), true, false);
        //mMesh.uv = atlasUVs;
        mMesh.name = mFilter.name + "_" + mfs.Length + "_" + uvCount.ToString();
        mFilter.sharedMesh = mMesh;
        if(error.Length > 0)
        {
            error.Insert(0,"缩放反转错误：\n");
            Debug.LogError(error.ToString());
        }
        //mMesh.RecalculateBounds();
        //Debug.Log("combine meshes takes : " + (Time.realtimeSinceStartup - startTime) * 1000 + " ms");
    }
}
