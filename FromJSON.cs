using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.Presets; 
using System;

public class FromJSON : MonoBehaviour
{
    string obj_path;
    string jsonString;


    // RegisterCollisionMask OPENS THE PHYSICS2DSETTINGS.ASSET FILE, WHICH
    // CONTAINS THE COLLISION MATRIX AND MODIFY IT =============================

    //void RegisterCollisionMask()
    //{
    //    // PHYSICS2DSETTINGS.ASSET FILE DIRECTORY ============================== 
    //    const string PhysSettingAssetPath = "ProjectSettings/Physics2DSettings.asset";

    //    // GET THE LAYER COLLISION MATRIX PROPERTY =============================
    //    SerializedObject PhysManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(PhysSettingAssetPath)[0]);
    //    SerializedProperty m_LayerCollisionMatrix = PhysManager.FindProperty("m_LayerCollisionMatrix");


    //    string newMatrix = string.Empty;
    //    for (int i = 0; i < m_LayerCollisionMatrix.arraySize; i++)
    //    {
    //        //if (i == catbits)
    //        //{
    //        SerializedProperty prop = m_LayerCollisionMatrix.GetArrayElementAtIndex(i);

    //        uint value = (uint)prop.intValue;

    //        byte[] bytes = BitConverter.GetBytes(value);
    //        Array.Reverse(bytes, 0, bytes.Length);

    //        uint converted_value = BitConverter.ToUInt32(bytes, 0);

    //        newMatrix += Convert.ToString(converted_value, 16);
    //    }



    //    //string matrix_path = "ProjectSettings/Physics2DSettings.asset";

    //    string line = "  m_LayerCollisionMatrix: " + newMatrix;

    //    var arrLine = File.ReadAllLines(PhysSettingAssetPath);


    //    arrLine[55] = line;

    //    File.WriteAllLines(PhysSettingAssetPath, arrLine);






    //}


    void Start()
    {
        
        // CREATE AND MODIFY LAYER LIST ========================================
        List<int> mask_list = new List<int>();

        for (int i = 8; i <= 31; i++)
        {
            string layername = LayerMask.LayerToName(i);
            if (layername.Length > 0)
            {
                int layernumber = LayerMask.NameToLayer(layername);
                //Debug.Log("Layer Name: " + layername);
                mask_list.Add(layernumber);
            }

        }


        // LIST OF OBJECT USED =================================================


        List<string> File_name = new List<string>(){
        "ball",
        "circle",
        "corner_bottom_left_1",
        "corner_bottom_left_2",
        "corner_bottom_right_1",
        "corner_bottom_right_2",
        "corner_top_left_1",
        "corner_top_left_2",
        "corner_top_right_1",
        "corner_top_right_2",
        "rectangle",
        "rugbyball",
        "square",
        "target",
        "triangle",
        "triangle_2a",
        "triangle_2b",
        "triangle_wood",
        "panel",
        "paddle"
              };




        //List<string> File_name = new List<string>(){
        //  "ball",

        //};


        // FOR EACH OBJECT IN LIST, CREATE ONE PARENT GO =======================

        foreach (string object_name in File_name)
        {

            // READ JSON FILE AND CHANGE "-" TO "_" ============================

            obj_path = Application.streamingAssetsPath + "/" + "json" + "/" + object_name + ".json"; ;
            jsonString = File.ReadAllText(obj_path);

            jsonString = jsonString.Replace("filter-", "filter_");

            World Json_object = JsonUtility.FromJson<World>(jsonString);


            GameObject parentobject = new GameObject
            {
                name = object_name
                
            };


            // CREATE LIST OF OBJECT IN THE BODY ===============================

            List<GameObject> list_go = new List<GameObject>(); // create list of object. 



            // CREATE 1 GO FOR EACH BODY =======================================

            foreach (Body body in Json_object.body)
            {
                //INITIATE GO ==================================================

                GameObject obj_go = new GameObject
                {
                    name = "body_" + body.name 
                };

                list_go.Add(obj_go);







                //// ADD RIGIDBODY =============================================

                Rigidbody2D obj_rigidbody = obj_go.AddComponent<Rigidbody2D>();



                // RIGIDBODY PROPERTIES ========================================

                obj_rigidbody.angularVelocity = body.angularVelocity;

                obj_rigidbody.angularDrag = body.angularDamping;

                obj_rigidbody.bodyType = body.type == 0 ? RigidbodyType2D.Static : body.type == 1 ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;

                obj_rigidbody.useAutoMass = true;

                obj_rigidbody.gravityScale = body.gravityScale;


                if (body.allowSleep)
                {
                    if (body.awake)
                    {
                        obj_rigidbody.sleepMode = RigidbodySleepMode2D.StartAwake;
                    }
                    else
                    {
                        obj_rigidbody.sleepMode = RigidbodySleepMode2D.StartAsleep;
                    }
                }
                else
                {
                    obj_rigidbody.sleepMode = RigidbodySleepMode2D.NeverSleep;
                }



                // CHECK IF THERE'S PHYSICS MATERIAL ALREADY AVAILABLE OR NOT.
                // IF NOT, CREATE ONE. =========================================


                string filePath = "Assets/PhysicsMaterial/" + object_name;

                try
                {
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                }
                catch (IOException ex)
                {
                    Debug.Log(ex.Message);
                };


                try
                {
                    string asset_path = filePath + "/" + body.name + ".physicsMaterial2D";

                    if (File.Exists(asset_path))
                    {
                        obj_rigidbody.sharedMaterial = (PhysicsMaterial2D)AssetDatabase.LoadAssetAtPath(asset_path, typeof(PhysicsMaterial2D));
                    }
                    else
                    {
                        PhysicsMaterial2D mat = new PhysicsMaterial2D
                        {
                            bounciness = body.fixture[0].restitution,
                            friction = body.fixture[0].friction
                        };

                        AssetDatabase.CreateAsset(mat, filePath + "/" + body.name + ".physicsMaterial2D");

                        obj_rigidbody.sharedMaterial = mat;
                    }
                }
                catch (IOException ex)
                {
                    Debug.Log(ex.Message);
                };


                // FOR EACH FIXTURE IN THE BODY, CREATE ONE GAME OBJECT WITH
                // COLLIDER ====================================================

                foreach (Fixture f in body.fixture)
                {
                    GameObject fixture_go = new GameObject
                    {
                        name = "collider_" + f.name,

                    };


                    // ADD THE GO TO THE LIST ======================================

                    fixture_go.transform.parent = obj_go.transform;

                    //fixture_go.transform.position = new Vector2(body.position.x, body.position.y);


                    // MASKING COLLIDERS, PUT THEM IN LAYERS AND EDIT COLLISION
                    // MATRIX ==================================================

                    int maskbits = f.filter_maskBits;

                    int catbits = f.filter_categoryBits + 8;

                    fixture_go.layer = catbits;


                    foreach (int alayer in mask_list)

                    {
                        int compare_bits = alayer == 8 ? 0 : 1 << (alayer - 9);

                        //Debug.Log(alayer + " - Compare Bits:" + compare_bits);

                        if ((compare_bits & maskbits) > 0)
                        {
                            Physics2D.IgnoreLayerCollision(alayer, catbits, false);
                        }
                        else
                        {
                            Physics2D.IgnoreLayerCollision(alayer, catbits, true);
                        }
                    }

                    //RegisterCollisionMask();



                    if (f.circle.radius != 0)
                    {
                        CircleCollider2D obj_circlecollider = fixture_go.AddComponent<CircleCollider2D>();
                        obj_circlecollider.radius = f.circle.radius;
                        obj_circlecollider.offset = new Vector2(f.circle.center.x, f.circle.center.y);
                        if (f.sensor)
                        {
                            obj_circlecollider.isTrigger = true;
                        }

                    }
                    else if (f.polygon.vertices != null)
                    {
                        // Create polygon colliders --------------------------
                        PolygonCollider2D obj_polygoncollider = fixture_go.AddComponent<PolygonCollider2D>();

                        // Create path to polygon colliders ----------------
                        List<Vector2> path = new List<Vector2>();

                        for (int i = 0; i < f.polygon.vertices.x.Count; i++)
                        {
                            float x = f.polygon.vertices.x[i];
                            float y = f.polygon.vertices.y[i];
                            Vector2 p = new Vector2(x, y);
                            path.Insert(i, p);

                        }

                        obj_polygoncollider.SetPath(0, path);


                        if (f.sensor)
                        {
                            obj_polygoncollider.isTrigger = true;
                        }
                    }
                }


                // CHILD THE CREATED GO TO THE PARENT GO =======================

                obj_go.transform.parent = parentobject.transform;


                // SET BODY POSITION (WITH REGARDS TO PARENT'S POSITION ========

                obj_go.transform.position = new Vector2(body.position.x, body.position.y);

                float rot_angle = Mathf.Rad2Deg * body.angle;
                obj_go.transform.eulerAngles = new Vector3(0, 0, rot_angle);

            }



            foreach (Joint joint in Json_object.joint)
            {


                // GET THE BODY THAT THE JOINT WILL BE ATTACHED TO =============
                //string attached_object_name = list_go[joint.bodyA];

                GameObject attached_go = list_go[joint.bodyA];


                // FIND CONNECTED RIGIDBODY ============================

                GameObject connected_go = list_go[joint.bodyB];
                Rigidbody2D connected_body = connected_go.GetComponent<Rigidbody2D>();



                //CREATE JOINT =================================================

                switch (joint.type)
                {


                    // FIRST CASE: REVOLUTE JOINT = HINGE JOINT IN UNITY ==========

                    case "revolute":

                        // INITIATE JOINT ======================================
                        HingeJoint2D joint_hinge = attached_go.gameObject.AddComponent<HingeJoint2D>();



                        // JOINT PROPERTIES ====================================

                        joint_hinge.connectedBody = connected_body;

                        joint_hinge.enableCollision = joint.collideConnected;

                        joint_hinge.autoConfigureConnectedAnchor = false;
                        joint_hinge.anchor = new Vector2(joint.anchorA.x, joint.anchorA.y);
                        joint_hinge.connectedAnchor = new Vector2(joint.anchorB.x, joint.anchorB.y);

                        joint_hinge.useMotor = joint.enableMotor;
                        JointMotor2D motor_rev = new JointMotor2D
                        {
                            motorSpeed = joint.motorSpeed,
                            maxMotorTorque = joint.maxMotorTorque
                        };

                        joint_hinge.motor = motor_rev;



                        JointAngleLimits2D hinge_angle_lim = new JointAngleLimits2D
                        {
                            max = joint.upperLimit,
                            min = joint.lowerLimit
                        };

                        joint_hinge.limits = hinge_angle_lim;

                        joint_hinge.useLimits = joint.enableLimit;

                        break;


                    // SECOND CASE: PRISMATIC JOINT = SLIDER JOINT IN UNITY =======

                    case "prismatic":


                        //INITIATE JOINT =======================================

                        SliderJoint2D joint_slider = attached_go.gameObject.AddComponent<SliderJoint2D>();

                        // JOINT PROPERTIES ====================================

                        joint_slider.connectedBody = connected_body;

                        joint_slider.enableCollision = joint.collideConnected;


                        joint_slider.autoConfigureConnectedAnchor = false;
                        joint_slider.anchor = new Vector2(joint.anchorA.x, joint.anchorA.y);
                        joint_slider.connectedAnchor = new Vector2(joint.anchorB.x, joint.anchorB.y);

                        joint_slider.useMotor = joint.enableMotor;
                        JointMotor2D motor_slider = new JointMotor2D
                        {
                            motorSpeed = joint.motorSpeed,
                            maxMotorTorque = joint.maxMotorTorque
                        };

                        joint_slider.motor = motor_slider;



                        JointTranslationLimits2D trans_lim = new JointTranslationLimits2D
                        {
                            max = joint.upperLimit,
                            min = joint.lowerLimit
                        };

                        joint_slider.limits = trans_lim;

                        joint_slider.useLimits = joint.enableLimit;

                        joint_slider.autoConfigureAngle = false;
                        joint_slider.angle = Mathf.Atan2(joint.localaxisA.y, joint.localaxisA.x);

                        break;


                    // THIRD CASE: DISTANCE JOINT = SPRING JOINT IN UNITY ==========
                    case "distance":


                        //  INITIATE JOINT =====================================

                        SpringJoint2D joint_spring = attached_go.gameObject.AddComponent<SpringJoint2D>();

                        // JOINT PROPERTIES ====================================

                        joint_spring.connectedBody = connected_body;

                        joint_spring.enableCollision = joint.collideConnected;

                        joint_spring.autoConfigureConnectedAnchor = false;
                        joint_spring.anchor = new Vector2(joint.anchorA.x, joint.anchorA.y);
                        joint_spring.connectedAnchor = new Vector2(joint.anchorB.x, joint.anchorB.y);

                        joint_spring.autoConfigureDistance = false;
                        joint_spring.distance = joint.length;

                        joint_spring.dampingRatio = joint.dampingRatio;

                        joint_spring.frequency = joint.frequency;


                        break;

                    case "weld":

                        FixedJoint2D joint_fixed = attached_go.gameObject.AddComponent<FixedJoint2D>();


                        joint_fixed.connectedBody = connected_body;

                        joint_fixed.enableCollision = joint.collideConnected;

                        joint_fixed.autoConfigureConnectedAnchor = false;
                        joint_fixed.anchor = new Vector2(joint.anchorA.x, joint.anchorA.y);
                        joint_fixed.connectedAnchor = new Vector2(joint.anchorB.x, joint.anchorB.y);

                        joint_fixed.dampingRatio = joint.dampingRatio;

                        joint_fixed.frequency = joint.frequency;



                        break;



                }



            }

            // SAVE THE PARENT OBJECT TO PREFAB AND THEN DESTROY IT IN SCENE ===

            PrefabUtility.SaveAsPrefabAsset(parentobject, "Assets/prefabs/" + object_name + ".prefab");
            Destroy(parentobject);

            //RegisterCollisionMask();

        }


    }


}




