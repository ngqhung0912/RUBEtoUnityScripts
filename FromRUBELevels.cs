using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class FromRUBELevels : MonoBehaviour
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
                mask_list.Add(layernumber);
            }

        }


        // LIST OF OBJECT USED =================================================


        List<string> File_name = new List<string>(){
            "p00_l00",
            "p00_l01",
            "p00_l02",
            "p00_l03",
            "p00_l04",
            "p00_l05",
            "p00_l06",
            "p00_l07",
            "p00_l08",
            "p00_l09",
              };

        // FOR EACH OBJECT IN LIST, CREATE ONE PARENT GO =======================

        foreach (string object_name in File_name)

        {

            // READ JSON FILE AND CHANGE "-" TO "_" ============================


            string obj_path = Application.streamingAssetsPath + "/" + "00" + "/" + object_name + ".rube";



            string rubeString = File.ReadAllText(obj_path);

            rubeString = rubeString.Replace("filter-", "filter_").
                Replace("massData-", "massData_").Replace("int", "value");



            Rubeworld Rube_world = JsonUtility.FromJson<Rubeworld>(rubeString);

            Metaworld Rube_object = Rube_world.metaworld;

            GameObject parentobject = new GameObject
            {
                name = object_name

            };


            // CREATE LIST OF OBJECT IN THE BODY ===============================

            Dictionary<int, GameObject> list_go = new Dictionary<int, GameObject>();

            // CREATE 1 GO FOR EACH BODY =======================================

            foreach (Metabody body in Rube_object.metabody)
            {
                GameObject obj_go = functions.GetbodyfromJSON(body, object_name);

                list_go.Add(body.id, obj_go);

                // FOR EACH FIXTURE IN THE BODY, CREATE ONE GAME OBJECT WITH
                // COLLIDER ====================================================

                foreach (Fixture_rube f in body.fixture)
                {
                    functions.GetcolliderfromJSON(f, obj_go, mask_list);

                }
                // CHILD THE CREATED GO TO THE PARENT GO =======================

                obj_go.transform.parent = parentobject.transform;


                // SET BODY POSITION (WITH REGARDS TO PARENT'S POSITION ========

                obj_go.transform.position = new Vector2(body.position.x, body.position.y);

                float rot_angle = Mathf.Rad2Deg * body.angle;
                obj_go.transform.eulerAngles = new Vector3(0, 0, rot_angle);

            }

            foreach (Metaobject child_obj in Rube_object.metaobject)
            {
                functions.GetPrefabfromRUBE(child_obj, parentobject);
            }


            // SAVE THE PARENT OBJECT TO PREFAB AND THEN DESTROY IT IN SCENE ===
            string level_prefab_directory = "Assets/RUBE_levels_prefab";

            if (!Directory.Exists(level_prefab_directory))
            {
                AssetDatabase.CreateFolder("Assets", "RUBE_levels_prefab");
            }

            PrefabUtility.SaveAsPrefabAsset(parentobject, level_prefab_directory + "/" + object_name + ".prefab");
            Destroy(parentobject);


        }


    }

}