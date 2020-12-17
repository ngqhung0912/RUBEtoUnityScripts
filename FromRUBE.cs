using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.Presets; 
using System;

public class FromRUBE : MonoBehaviour
{

    void Start()
    {

        Functions functions = GetComponent<Functions>();

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
        "ball_big",
        "basket",
        "bear",
        "circle",
        "controller",
        "corner_top_left_2",
        "corner_bottom_left_1",
        "corner_bottom_left_2",
        "corner_bottom_right_1",
        "corner_bottom_right_2",
        "corner_top_left_1",
        "corner_top_right_1",
        "corner_top_right_2",
        "paddle_mid",
        "paddle_mid_reverse",
        "paddle_long",
        "paddle_long_reverse",
        "paddle_short",
        "paddle_short_reverse",
        "paddle_short_left",
        "paddle_short_right",
        "panel",
        "rectangle",
        "square",
        "squid",
        "target",
        "triangle",
        "triangle_2a",
        "triangle_2b",
        "screw_blue",
        "screw_gray",
        "screw_yellow"
          };
        // FOR EACH OBJECT IN LIST, CREATE ONE PARENT GO =======================

        foreach (string object_name in File_name)

        {

            // READ JSON FILE AND CHANGE "-" TO "_" ============================


            string obj_path = Application.streamingAssetsPath + "/" + "rube/objects" + "/" + object_name + ".rube";

            string rubeString = File.ReadAllText(obj_path);

            rubeString = rubeString.
                Replace("filter-", "filter_").
                Replace("massData-", "massData_").
                Replace("\"int\" :", "\"intValue\" :").
                Replace("\"float\" :", "\"floatValue\" :").
                Replace("\"string\" :", "\"stringValue\" :").
                Replace("\"bool\" :", "\"boolValue\" :")
                ;



            Rubeworld Rube_world = JsonUtility.FromJson<Rubeworld>(rubeString);

            Metaworld Rube_object = Rube_world.metaworld;

            GameObject parentobject = new GameObject
            {
                name = object_name 

            };



            foreach (Metaobject child_obj in Rube_object.metaobject)
            {
                 functions.GetPrefabfromRUBE(child_obj, parentobject);

            }

            // CREATE LIST OF OBJECT IN THE BODY ===============================

            //List<GameObject> list_go = new List<GameObject>(); // create list of object. 

            Dictionary<int, GameObject> list_go = new Dictionary<int, GameObject>();

            // CREATE 1 GO FOR EACH BODY =======================================

            foreach (Metabody body in Rube_object.metabody)
            {

                GameObject obj_go = functions.GetbodyfromJSON(body, object_name);

                list_go.Add(body.id, obj_go);


                // FOR EACH FIXTURE IN THE BODY, CREATE ONE GAME OBJECT WITH
                // COLLIDER (USING FUNCTIONS)===================================
                GameObject phys_go = new GameObject
                {
                    name = "body"
                };

                foreach (Fixture_rube f in body.fixture)
                {
                   GameObject fixture_go = functions.GetcolliderfromJSON(f, obj_go, mask_list);
                    fixture_go.transform.parent = phys_go.transform;

                }
                // CHILD THE CREATED GO TO THE PARENT GO =======================
                phys_go.transform.parent = obj_go.transform;
                obj_go.transform.parent = parentobject.transform;


                // SET BODY POSITION (WITH REGARDS TO PARENT'S POSITION ========


                float rot_angle = Mathf.Rad2Deg * body.angle;
                obj_go.transform.eulerAngles = new Vector3(0, 0, rot_angle);

          
                // GRAPHICS ====================================================


                GameObject graphics_go = new GameObject
                {
                    name = "graphics"
                };
                graphics_go.transform.parent = obj_go.transform;


                string body_name = obj_go.name.Replace("_" + body.id.ToString("00"), "");

                string graphics_directory = "Assets/bodies/" + body_name + "/static/" + body_name + ".png";


                if (File.Exists(graphics_directory))
                {
                    SpriteRenderer graphics_renderer = graphics_go.AddComponent<SpriteRenderer>();
                    graphics_renderer.sprite = (Sprite)AssetDatabase.LoadAssetAtPath(graphics_directory, typeof(Sprite));
                }
                
                graphics_go.transform.localScale = new Vector3(0.25f, 0.25f, 0);
                graphics_go.transform.eulerAngles = new Vector3(0, 0, rot_angle);

                int graphics_layer = -body.customProperties[0].intValue;

                obj_go.transform.position = new Vector3(body.position.x, body.position.y, graphics_layer);

            }


           


            // CREATE JOINTS FOR BODIES
            foreach (Joint joint in Rube_object.metajoint)
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

                switch (joint.type)
                {
                    case "revolute":
                        {

                            break;
                        }
                }

            }
            string prefab_directory = "Assets/RUBE_prefabs/"; 

            if (!Directory.Exists(prefab_directory))
            {
                AssetDatabase.CreateFolder("Assets", "RUBE_prefabs");
            }
            PrefabUtility.SaveAsPrefabAsset(parentobject, prefab_directory + "/" + object_name + ".prefab");
            Destroy(parentobject);


        }

    }
}






