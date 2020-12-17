using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
// FOR JSON ==================================================================== 

[System.Serializable]
public class World
{

    public bool allowSleep;
    public string name;

    public List<Body> body;
    public List<Joint> joint;
}

[System.Serializable]
public class Body
{

    public float angularDamping;
    public float angularVelocity;
    public bool awake;
    public bool allowSleep;
    public float angle;


    public List<Fixture> fixture;



    public float gravityScale;
    public float linearVelocity;
    public RubeVector2 position;
    public int type;
    public string name;
    public int index;

}


// FOR RUBE ====================================================================

[System.Serializable]

public class Rubeworld
{
    public Metaworld metaworld;
}
[System.Serializable]

public class Metaworld
{

    public bool allowSleep;
    public string name;

    public bool autoClearForces;
    public bool continuousPhysics;
    public RubeVector2 gravity;


    public List<Metabody> metabody;
    public List<Joint> metajoint;
    public List<Metaobject> metaobject;

}

[System.Serializable]


public class Metabody
{

    public float angularDamping;
    public float angularVelocity;
    public bool awake;
    public bool sleepingAllowed;
    public float angle;
    public bool bullet;

    public List<CustomProperties> customProperties;

    public float massData_mass;
    public float massData_I;

    public List<Fixture_rube> fixture;



    public float gravityScale;
    public float linearVelocity;
    public RubeVector2 position;
    public string type;
    public string name;
    public int id;

}


// FOR JSON ==================================================================== 
[System.Serializable]
public class Fixture
{
    public float density;
    public int filter_maskBits;
    public int filter_categoryBits;
    public float friction;
    public string name;
    public float restitution;

    public Circle circle;
    public Polygon polygon;
    public bool sensor; 

}

[System.Serializable]
public class Circle
{
    public RubeVector2 center;
    public float radius;
}

[System.Serializable]

public class Polygon
{
    public Vertices vertices;
}

[System.Serializable]
public class Vertices
{
    public List<float> x;
    public List<float> y;
}


// FOR RUBE ====================================================================

[System.Serializable]
public class Fixture_rube
{
    public float density;
    public int filter_maskBits;
    public int filter_categoryBits;
    public float friction;
    public string name;
    public float restitution;
    public int id;
    public List<Shapes> shapes;
    public Vertices vertices;

    public bool sensor;

}

[System.Serializable]
public class Shapes
{
    public string type;
    public float radius;
}


// JOINT FOR ALL ===============================================================
[System.Serializable]

public class Joint
{
    public string name;
    public string type;
    public int bodyA;
    public int bodyB;
    public bool collideConnected;

    public bool enableLimit;
    public bool enableMotor;

    public int id;

    public float lowerLimit;
    public float upperLimit;

    public RubeVector2 anchorA;
    public RubeVector2 anchorB;


    public RubeVector2 localaxisA;

    public float dampingRatio;
    public float frequency;
    public float length;

    public float joinSpeed;
    public float motorSpeed;
    public float maxMotorTorque;


    public float refAngle;
}

[System.Serializable]


public class RubeVector2
{
    public float x;
    public float y;
}


// METAOBJECT for RUBE Levels ==================================================

[System.Serializable]

public class Metaobject
{
    public float angle;
    public string file;
    public bool flip;
    public int id;
    public string name;
    public RubeVector2 position;
    public float scale;
    public List<CustomProperties> customProperties;
}

[System.Serializable]

public class CustomProperties
{
    public int intValue;
    public float floatValue;
    public string stringValue;
    public bool boolValue;
    public string name; 
}

public class Functions : MonoBehaviour
{
    public void GetPrefabfromRUBE(Metaobject child_obj, GameObject parentobject)
    {
        string prefab_directory = "Assets/RUBE_prefabs" + "/" + child_obj.file.Replace(".rube", ".prefab");

        GameObject prefab_go = (GameObject)AssetDatabase.LoadAssetAtPath(prefab_directory, typeof(GameObject));


        GameObject level_object = Instantiate(prefab_go, transform);

        level_object.name = child_obj.file.Replace(".rube", "(prefab)");

        int graphics_layer = -child_obj.customProperties[0].intValue;

        level_object.transform.position = new Vector3(child_obj.position.x, child_obj.position.y, graphics_layer);
        level_object.transform.parent = parentobject.transform;
        int flip = child_obj.flip == true ? 180 : 0;
        level_object.transform.eulerAngles = new Vector3(0, flip, Mathf.Rad2Deg * child_obj.angle);

    }



    public GameObject GetbodyfromJSON(Metabody body, string object_name)
    {
        GameObject obj_go = new GameObject
        {
            name =  body.name + "_" + body.id.ToString("00")
        };

        //// ADD RIGIDBODY =============================================

        Rigidbody2D obj_rigidbody = obj_go.AddComponent<Rigidbody2D>();


        // RIGIDBODY PROPERTIES ========================================

        obj_rigidbody.angularVelocity = body.angularVelocity;

        obj_rigidbody.angularDrag = body.angularDamping;

        obj_rigidbody.bodyType = body.type == "static" ? RigidbodyType2D.Static : body.type == "kinematic" ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;

        obj_rigidbody.useAutoMass = false;

        obj_rigidbody.mass = body.massData_mass;


        obj_rigidbody.gravityScale = body.gravityScale;


        if (body.sleepingAllowed)
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

        return obj_go;
            
    }




    public GameObject GetcolliderfromJSON(Fixture_rube f, GameObject obj_go, List<int> mask_list)
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



        if (f.shapes[0].type == "circle")
        {
            CircleCollider2D obj_circlecollider = fixture_go.AddComponent<CircleCollider2D>();
            obj_circlecollider.radius = f.shapes[0].radius;
            obj_circlecollider.offset = new Vector2(f.vertices.x[0], f.vertices.y[0]);
            if (f.sensor)
            {
                obj_circlecollider.isTrigger = true;
            }

        }   
        else if (f.shapes[0].type == "polygon")
        {
            // Create polygon colliders --------------------------
            PolygonCollider2D obj_polygoncollider = fixture_go.AddComponent<PolygonCollider2D>();

            // Create path to polygon colliders ----------------
            List<Vector2> path = new List<Vector2>();

            for (int i = 0; i < f.vertices.x.Count; i++)
            {
                float x = f.vertices.x[i];
                float y = f.vertices.y[i];
                Vector2 p = new Vector2(x, y);
                path.Insert(i, p);

            }

            obj_polygoncollider.SetPath(0, path);


            if (f.sensor)
            {
                obj_polygoncollider.isTrigger = true;
            }
        }
        else if (f.shapes[0].type == "line")
        {
            // Create line colliders --------------------------
            EdgeCollider2D obj_edgecollider = fixture_go.AddComponent<EdgeCollider2D>();

            // Create path to polygon colliders ----------------
            List<Vector2> path = new List<Vector2>();

            for (int i = 0; i < f.vertices.x.Count; i++)
            {
                float x = f.vertices.x[i];
                float y = f.vertices.y[i];
                Vector2 p = new Vector2(x, y);
                path.Insert(i, p);

            }

            obj_edgecollider.points = path.ToArray();


            if (f.sensor)
            {
                obj_edgecollider.isTrigger = true;
            }
        }

        return fixture_go; 
    }

    public float Getextremefromlist(List<float> elem_list,string maxormin)
    {
        float extreme_elem = 0;

        if (maxormin == "max")
        {
            foreach (float elem in elem_list)
            {
                if (elem > extreme_elem)
                {
                    extreme_elem = elem;
                }
            }

        }
        else if (maxormin == "min")
        {
            foreach (float elem in elem_list)
            {
                if (elem < extreme_elem)
                {
                    extreme_elem = elem;
                }
            }
        }
        return extreme_elem;

    }


    public Vector2 CalculateFixtureOffset(Metabody body)
    {
        Vector2 offset = new Vector2(0, 0);


        List<float> x_max = new List<float>();
        List<float> y_max = new List<float>();
        List<float> x_min = new List<float>();
        List<float> y_min = new List<float>();


        foreach (Fixture_rube f in body.fixture)
        {
            switch (f.shapes[0].type)
            {
                case "circle":

                    x_max.Add(f.vertices.x[0] + f.shapes[0].radius / Mathf.Sqrt(2));

                    x_min.Add(f.vertices.x[0] - f.shapes[0].radius / Mathf.Sqrt(2));

                    y_max.Add(f.vertices.y[0] + f.shapes[0].radius / Mathf.Sqrt(2));

                    y_min.Add(f.vertices.x[0] - f.shapes[0].radius / Mathf.Sqrt(2));

                    break;
                case "polygon":
                    float x_max_polygon = 0;
                    float x_min_polygon = 0;
                    float y_max_polygon = 0;
                    float y_min_polygon = 0;

                    foreach (float point_x in  f.vertices.x)
                    {
                        if (point_x > x_max_polygon)
                        {
                            x_max_polygon = point_x;
                        }
                        if (point_x < x_min_polygon)
                        {
                            x_min_polygon = point_x; 
                        }

                    }
                    foreach (float point_y in f.vertices.y)
                    {
                        if (point_y > y_max_polygon)
                        {
                            y_max_polygon = point_y;
                        }
                        if (point_y < y_min_polygon)
                        {
                            y_min_polygon = point_y;
                        }
                    }
                    x_max.Add(x_max_polygon);

                    x_min.Add(x_min_polygon);

                    y_max.Add(y_max_polygon);

                    y_min.Add(y_min_polygon);

                    break;
            }



        }
        float max_x = Getextremefromlist(x_max, "max");
        float max_y = Getextremefromlist(y_max, "max");
        float min_x = Getextremefromlist(x_min, "min");
        float min_y = Getextremefromlist(x_min, "min");

        offset.x = (max_x + min_x) / 2;
        offset.y = (max_y + min_y) / 2; 


        return offset; 
    }


}