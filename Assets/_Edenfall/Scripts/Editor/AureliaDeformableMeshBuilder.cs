#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.U2D;

namespace Edenfall.Editor
{
    /// <summary>
    /// Phase 3.6.4: Builds valid 2D mesh geometry, bone bindings, bind poses,
    /// and normalized vertex weights for Aurelia deformable hair and clothing layers.
    /// Integrates directly with Unity 2D Animation SpriteSkin.
    /// </summary>
    public static class AureliaDeformableMeshBuilder
    {
        private const string RigLayersPath = "Assets/_Edenfall/Art/Characters/Aurelia/RigLayers";

        public struct HairMeshDataConfig
        {
            public string fileName;
            public int cols;
            public int rows;
            public float minX;
            public float maxX;
            public float minY;
            public float maxY;
            public SpriteBone[] bones;
        }

        public static HairMeshDataConfig[] GetHairConfigs()
        {
            return new HairMeshDataConfig[]
            {
                // 1. Left Side Hair (Layer_07_Hair_SideL)
                new HairMeshDataConfig
                {
                    fileName = "Layer_07_Hair_SideL",
                    cols = 6,
                    rows = 16,
                    minX = 670f,
                    maxX = 950f,
                    minY = 1535f,
                    maxY = 2364f,
                    bones = new SpriteBone[]
                    {
                        new SpriteBone
                        {
                            name = "Bone_HairSideL_01",
                            position = new Vector3(837f, 2242f, 0f),
                            rotation = Quaternion.identity,
                            length = 280f,
                            parentId = -1,
                            color = new Color32(80, 255, 120, 255)
                        },
                        new SpriteBone
                        {
                            name = "Bone_HairSideL_02",
                            position = new Vector3(-25f, -274f, 0f),
                            rotation = Quaternion.identity,
                            length = 270f,
                            parentId = 0,
                            color = new Color32(80, 200, 255, 255)
                        },
                        new SpriteBone
                        {
                            name = "Bone_HairSideL_03",
                            position = new Vector3(-41f, -262f, 0f),
                            rotation = Quaternion.identity,
                            length = 200f,
                            parentId = 1,
                            color = new Color32(255, 120, 220, 255)
                        }
                    }
                },

                // 2. Right Side Hair (Layer_07_Hair_SideR)
                new HairMeshDataConfig
                {
                    fileName = "Layer_07_Hair_SideR",
                    cols = 6,
                    rows = 16,
                    minX = 1350f,
                    maxX = 1670f,
                    minY = 1480f,
                    maxY = 2360f,
                    bones = new SpriteBone[]
                    {
                        new SpriteBone
                        {
                            name = "Bone_HairSideR_01",
                            position = new Vector3(1442f, 2214f, 0f),
                            rotation = Quaternion.identity,
                            length = 280f,
                            parentId = -1,
                            color = new Color32(80, 255, 120, 255)
                        },
                        new SpriteBone
                        {
                            name = "Bone_HairSideR_02",
                            position = new Vector3(32f, -274f, 0f),
                            rotation = Quaternion.identity,
                            length = 280f,
                            parentId = 0,
                            color = new Color32(80, 200, 255, 255)
                        },
                        new SpriteBone
                        {
                            name = "Bone_HairSideR_03",
                            position = new Vector3(83f, -285f, 0f),
                            rotation = Quaternion.identity,
                            length = 200f,
                            parentId = 1,
                            color = new Color32(255, 120, 220, 255)
                        }
                    }
                },

                // 3. Front Bangs (Layer_08_Hair_Front_Bangs)
                new HairMeshDataConfig
                {
                    fileName = "Layer_08_Hair_Front_Bangs",
                    cols = 10,
                    rows = 12,
                    minX = 785f,
                    maxX = 1415f,
                    minY = 2159f,
                    maxY = 2601f,
                    bones = new SpriteBone[]
                    {
                        new SpriteBone
                        {
                            name = "Bone_HairFront_01",
                            position = new Vector3(1127f, 2523f, 0f),
                            rotation = Quaternion.identity,
                            length = 130f,
                            parentId = -1,
                            color = new Color32(80, 255, 120, 255)
                        },
                        new SpriteBone
                        {
                            name = "Bone_HairFront_02",
                            position = new Vector3(-39f, -128f, 0f),
                            rotation = Quaternion.identity,
                            length = 150f,
                            parentId = 0,
                            color = new Color32(80, 200, 255, 255)
                        },
                        new SpriteBone
                        {
                            name = "Bone_HairFront_03",
                            position = new Vector3(-31f, -148f, 0f),
                            rotation = Quaternion.identity,
                            length = 120f,
                            parentId = 1,
                            color = new Color32(255, 120, 220, 255)
                        }
                    }
                },

                // 4. Back Flowing Hair (Layer_00_Hair_Back)
                new HairMeshDataConfig
                {
                    fileName = "Layer_00_Hair_Back",
                    cols = 12,
                    rows = 18,
                    minX = 480f,
                    maxX = 1680f,
                    minY = 980f,
                    maxY = 2700f,
                    bones = new SpriteBone[]
                    {
                        new SpriteBone
                        {
                            name = "Bone_HairBack_01",
                            position = new Vector3(1065f, 2413f, 0f),
                            rotation = Quaternion.identity,
                            length = 520f,
                            parentId = -1,
                            color = new Color32(80, 255, 120, 255)
                        },
                        new SpriteBone
                        {
                            name = "Bone_HairBack_02",
                            position = new Vector3(-126f, -515f, 0f),
                            rotation = Quaternion.identity,
                            length = 530f,
                            parentId = 0,
                            color = new Color32(80, 200, 255, 255)
                        },
                        new SpriteBone
                        {
                            name = "Bone_HairBack_03",
                            position = new Vector3(27f, -526f, 0f),
                            rotation = Quaternion.identity,
                            length = 400f,
                            parentId = 1,
                            color = new Color32(255, 120, 220, 255)
                        }
                    }
                }
            };
        }

        [MenuItem("Edenfall/Bake All Deformable Hair Meshes")]
        public static void BakeAllDeformableMeshes()
        {
            var configs = GetHairConfigs();
            foreach (var cfg in configs)
            {
                BakeSingleDeformableMesh(cfg);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AureliaDeformableMeshBuilder] Successfully baked 2D deformable meshes and weights for all hair layers!");
        }

        public static void BakeSingleDeformableMesh(HairMeshDataConfig cfg)
        {
            string path = $"{RigLayersPath}/{cfg.fileName}.png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[AureliaDeformableMeshBuilder] Missing TextureImporter for {path}");
                return;
            }

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dp = factory.GetSpriteEditorDataProviderFromObject(importer);
            dp.InitSpriteEditorDataProvider();

            var rects = dp.GetSpriteRects();
            if (rects == null || rects.Length == 0)
            {
                Debug.LogError($"[AureliaDeformableMeshBuilder] No sprite rects for {path}");
                return;
            }
            var sprRect = rects[0];

            var boneDp = dp.GetDataProvider<ISpriteBoneDataProvider>();
            var meshDp = dp.GetDataProvider<ISpriteMeshDataProvider>();

            // 1. Assign Bones
            boneDp.SetBones(sprRect.spriteID, new List<SpriteBone>(cfg.bones));

            // 2. Generate Triangulated Grid Mesh with Normalized Bone Weights
            int cols = cfg.cols;
            int rows = cfg.rows;
            var vertList = new List<Vertex2DMetaData>(cols * rows);
            var indicesList = new List<int>((cols - 1) * (rows - 1) * 6);
            var edgesList = new List<Vector2Int>((cols - 1) * (rows - 1) * 4);

            for (int r = 0; r < rows; r++)
            {
                float ty = (float)r / (rows - 1); // 0 at bottom (tip), 1 at top (root)
                float py = Mathf.Lerp(cfg.minY, cfg.maxY, ty);

                // Smooth cubic hermite blend between bones
                float w0 = 0f, w1 = 0f, w2 = 0f;
                if (ty >= 0.5f)
                {
                    float t = (ty - 0.5f) / 0.5f; // [0, 1]
                    // Root vs Mid
                    w0 = t * t * (3f - 2f * t);
                    w1 = 1f - w0;
                    w2 = 0f;
                }
                else
                {
                    float t = ty / 0.5f; // [0, 1]
                    // Mid vs Tip
                    w1 = t * t * (3f - 2f * t);
                    w2 = 1f - w1;
                    w0 = 0f;
                }

                // Strictly ensure normalization: sum = 1.000000
                float sum = w0 + w1 + w2;
                if (sum > 0f)
                {
                    w0 /= sum;
                    w1 /= sum;
                    w2 /= sum;
                }
                else
                {
                    w0 = 1f;
                }

                var bw = new BoneWeight
                {
                    boneIndex0 = 0, weight0 = w0,
                    boneIndex1 = 1, weight1 = w1,
                    boneIndex2 = 2, weight2 = w2,
                    boneIndex3 = 0, weight3 = 0f
                };

                for (int c = 0; c < cols; c++)
                {
                    float tx = (float)c / (cols - 1);
                    float px = Mathf.Lerp(cfg.minX, cfg.maxX, tx);

                    vertList.Add(new Vertex2DMetaData
                    {
                        position = new Vector2(px, py),
                        boneWeight = bw
                    });
                }
            }

            for (int r = 0; r < rows - 1; r++)
            {
                for (int c = 0; c < cols - 1; c++)
                {
                    int i00 = r * cols + c;
                    int i10 = r * cols + (c + 1);
                    int i01 = (r + 1) * cols + c;
                    int i11 = (r + 1) * cols + (c + 1);

                    // Quad = 2 triangles
                    indicesList.Add(i00);
                    indicesList.Add(i01);
                    indicesList.Add(i10);

                    indicesList.Add(i10);
                    indicesList.Add(i01);
                    indicesList.Add(i11);

                    edgesList.Add(new Vector2Int(i00, i10));
                    edgesList.Add(new Vector2Int(i00, i01));
                    edgesList.Add(new Vector2Int(i10, i11));
                    edgesList.Add(new Vector2Int(i01, i11));
                }
            }

            meshDp.SetVertices(sprRect.spriteID, vertList.ToArray());
            meshDp.SetIndices(sprRect.spriteID, indicesList.ToArray());
            meshDp.SetEdges(sprRect.spriteID, edgesList.ToArray());

            dp.Apply();
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            Debug.Log($"[AureliaDeformableMeshBuilder] Baked {cfg.fileName}: {vertList.Count} vertices, {indicesList.Count} indices, {cfg.bones.Length} bones.");
        }
    }
}
#endif
