using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum TileType
{
    WALL = 0,
    FLOOR = 1,
    DRUG = 2,
    VIRUS = 3,
    COIN = 4
}

public class Level : MonoBehaviour
{
    // fields/variables you may adjust from Unity's interface
    public int width = 24;   // size of level (default 16 x 16 blocks)
    public int length = 24;
    public float storey_height = 2.5f;   // height of walls
    public float virus_speed = 2.5f;     // virus velocity
    public int num_viruses = 5; 
    public int num_tokens = 5;  
    public int damage_multi = 0;
    public GameObject fps_prefab;        // these should be set to prefabs as provided in the starter scene
    public GameObject virus_prefab;
    public GameObject house_prefab;
    public GameObject drug_prefab;
    public Material wallColor;
    public Material coinColor;
    public GameObject text_box;
    public Text text_tokens;
    public GameObject scroll_bar;
    public Camera cam;
    public AudioSource source;
    public AudioClip exit;
    public AudioClip infected;
    public AudioClip coin;
    public AudioClip drug;
    public UI PlayAgain;
    public GameObject tryAgain;

    // fields/variables accessible from other scripts
    internal GameObject fps_player_obj;   // instance of FPS template
    internal float player_health = 1.0f;  // player health in range [0.0, 1.0]
    internal bool virus_landed_on_player_recently = false;  // has virus hit the player? if yes, a timer of 5sec starts before infection
    internal bool drug_landed_on_player_recently = false;   // has drug collided with player?
    internal bool player_entered_house = false;             // has player arrived in house?
    internal int num_tokens_collected = 0; 

    // fields/variables needed only from this script
    private Bounds bounds;                   // size of ground plane in world space coordinates 
    private int function_calls = 0;          // number of function calls during backtracking for solving the CSP
    private List<int[]> pos_viruses;         // stores their location in the grid           
    private List<int[]> pos_tokens;
    private  List<int[]> pos_drugs;     
    private int[,] dist;
    private List<GameObject> obj;
    private List<TileType>[,] gridSol = null;
    private int wee_again = -1;
    private int lee_again = -1;
    private int wr_again = -1;
    private int lr_again = -1;

    // feel free to put more fields here, if you need them e.g, add AudioClips that you can also reference them from other scripts
    // for sound, make also sure that you have ONE audio listener active (either the listener in the FPS or the main camera, switch accordingly)

    // a helper function that randomly shuffles the elements of a list (useful to randomize the solution to the CSP)
    private void Shuffle<T>(ref List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        Time.timeScale = 1f;
        List<TileType>[,] grid = null;
        CreateGame(grid);
    }

    private void CreateGame(List<TileType>[,] grid) {
        // initialize internal/private variables
        bounds = GetComponent<Collider>().bounds; 
        function_calls = 0;
        player_health = 1.0f;
        virus_landed_on_player_recently = false;
        drug_landed_on_player_recently = false;
        player_entered_house = false;  
        num_tokens_collected = 0;   
        cam.GetComponent<AudioListener>().enabled = false;    
        dist = new int[width, length];
        obj = new List<GameObject>();

        if(grid != null) {
            DrawDungeon(grid);
        }
        else {
            grid = new List<TileType>[width, length];
            // useful to keep variables that are unassigned so far
            List<int[]> unassigned = new List<int[]>();
     
            pos_viruses = new List<int[]>();
            pos_tokens = new List<int[]>();
            pos_drugs = new List<int[]>(); 

            bool success = false;        
            while (!success)
            {

                for (int t = 0; t < num_tokens; t++)
                {
                    while (true) 
                    {
                        int wr = Random.Range(1, width - 1);
                        int lr = Random.Range(1, length - 1);
                        bool tooClose = false;

                        for(int p = 0; p < t; p++) {
                            if((System.Math.Abs(wr - pos_tokens[p][0]) + System.Math.Abs(lr - pos_tokens[p][1])) < 5) {
                                tooClose = true;
                                break;
                            }
                        }
                        
                        if (!tooClose && grid[wr, lr] == null)
                        {
                            grid[wr, lr] = new List<TileType> { TileType.COIN };
                            pos_tokens.Add(new int[2] { wr, lr });
                            break;
                        }
                    }
                }

                for (int v = 0; v < num_viruses; v++)
                {
                    while (true)
                    {
                        int wr = Random.Range(1, width - 1);
                        int lr = Random.Range(1, length - 1);
                        bool tooClose = false;

                        for(int p = 0; p < v; p++) {
                            if((System.Math.Abs(wr - pos_viruses[p][0]) + System.Math.Abs(lr - pos_viruses[p][1])) < 5) {
                                tooClose = true;
                                break;
                            }
                        }

                        if (!tooClose && grid[wr, lr] == null)
                        {
                            grid[wr, lr] = new List<TileType> { TileType.VIRUS };
                            pos_viruses.Add(new int[2] { wr, lr });
                            break;
                        }
                    }
                }

                for (int d = 0; d < 5; d++)
                {
                    while (true)
                    {
                        int wr = Random.Range(1, width - 1);
                        int lr = Random.Range(1, length - 1);

                        if (grid[wr, lr] == null)
                        {
                            grid[wr, lr] = new List<TileType> { TileType.DRUG };
                            pos_drugs.Add(new int[2] { wr, lr });
                            break;
                        }
                    }
                }

                for (int w = 0; w < width; w++)
                    for (int l = 0; l < length; l++)
                        if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                            grid[w, l] = new List<TileType> { TileType.WALL };
                        else
                        {
                            if (grid[w, l] == null) // does not have virus already or some other assignment from previous run
                            {
                                // CSP will involve assigning variables to one of the following four values (VIRUS is predefined for some tiles)
                                List<TileType> candidate_assignments = new List<TileType> { TileType.WALL, TileType.FLOOR };
                                Shuffle<TileType>(ref candidate_assignments);

                                grid[w, l] = candidate_assignments;
                                unassigned.Add(new int[] { w, l });
                            }
                        }

                // YOU MUST IMPLEMENT this function!!!
                success = BackTrackingSearch(grid, unassigned);
                if (!success)
                {
                    Debug.Log("Could not find valid solution - will try again");
                    unassigned.Clear();
                    grid = new List<TileType>[width, length];
                    function_calls = 0; 
                }
            }
            gridSol = grid;
            DrawDungeon(grid);
        }
        
    }

    bool TooManyInteriorWalls(List<TileType>[,] grid)
    {
        int[] number_of_assigned_elements = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                if (grid[w, l].Count == 1)
                    number_of_assigned_elements[(int)grid[w, l][0]]++;
            }

        if (number_of_assigned_elements[(int)TileType.WALL] > num_viruses * 10)
            return true;
        else
            return false;
    }

    bool TooFewWalls(List<TileType>[,] grid)
    {
        int[] number_of_potential_assignments = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                for (int i = 0; i < grid[w, l].Count; i++)
                    number_of_potential_assignments[(int)grid[w, l][i]]++;
            }

        if (number_of_potential_assignments[(int)TileType.WALL] < (width * length) / 4)
            return true;
        else
            return false;
    }

    bool TokeninCornerLike(List<TileType>[,] grid)
    {
        bool noWall = true;
        int numWalls = 0;
        for (int d = 0; d < pos_tokens.Count; d++) {
            int w = pos_tokens[d][0];
            int l =  pos_tokens[d][1];
            if(grid[w, l][0] == TileType.COIN) {
                for(int i = -1; i <= 1; i++) {
                    for(int j = -1; j <= 1; j++) {
                        if(i == 0 && j == 0)
                            continue;
                        if(w+i >= 0 && l+j >= 0 && w+i < width && l+j < length && grid[w, l].Count == 1 && grid[w+i,l+j][0] == TileType.WALL) { numWalls++; }
                        if(numWalls >= 4) { noWall = false; }
                    }
                }
                if(noWall) { return true; }
                noWall = true;
                numWalls = 0;
            }
        }
        return false;
    }


    // check if attempted assignment is consistent with the constraints or not
    bool CheckConsistency(List<TileType>[,] grid, int[] cell_pos, TileType t)
    {
        int w = cell_pos[0];
        int l = cell_pos[1];

        List<TileType> old_assignment = new List<TileType>();
        old_assignment.AddRange(grid[w, l]);
        grid[w, l] = new List<TileType> { t };

		// note that we negate the functions here i.e., check if we are consistent with the constraints we want
        bool areWeConsistent = !TooFewWalls(grid) && !TooManyInteriorWalls(grid) && !TokeninCornerLike(grid);

        grid[w, l] = new List<TileType>();
        grid[w, l].AddRange(old_assignment);
        return areWeConsistent;
    }


    // implement backtracking 
    bool BackTrackingSearch(List<TileType>[,] grid, List<int[]> unassigned)
    {
        // if there are too many recursive function evaluations, then backtracking has become too slow (or constraints cannot be satisfied)
        // to provide a reasonable amount of time to start the level, we put a limit on the total number of recursive calls
        // if the number of calls exceed the limit, then it's better to try a different initialization
        if (function_calls++ > 100000)       
            return false;

        // we are done!
        if (unassigned.Count == 0)
            return true;
        
        int random = Random.Range(0, unassigned.Count);
        int[] var = unassigned[random];
        unassigned.RemoveAt(random);
        for(int j = 0; j < grid[var[0], var[1]].Count; j++) {
            if(CheckConsistency(grid, var, grid[var[0], var[1]][j])) {
                List<TileType> old_assignment = new List<TileType>();
                old_assignment.AddRange(grid[var[0], var[1]]);
                grid[var[0], var[1]] = new List<TileType> { grid[var[0], var[1]][j] };
                if(BackTrackingSearch(grid, unassigned)) return true;
                grid[var[0], var[1]] = new List<TileType>();
                grid[var[0], var[1]].AddRange(old_assignment);
            }
        }
        return false;
    }


    // places the primitives/objects according to the grid assignents
    // you will need to edit this function (see below)
    void DrawDungeon(List<TileType>[,] solution)
    {
        GetComponent<Renderer>().material.color = Color.grey; // ground plane will be grey

        // place character at random position (wr, lr) in terms of grid coordinates (integers)
        // make sure that this random position is a FLOOR tile (not wall, drug, or virus)
        int wr = 0;
        int lr = 0;
        if(wr_again != -1 && lr_again != -1) {
            wr = wr_again;
            lr = lr_again;
            if (solution[wr, lr][0] == TileType.FLOOR)
            {
                float x = bounds.min[0] + (float)wr * (bounds.size[0] / (float)width);
                float z = bounds.min[2] + (float)lr * (bounds.size[2] / (float)length);
                fps_player_obj = Instantiate(fps_prefab);
                fps_player_obj.name = "PLAYER";
                // character is placed above the level so that in the beginning, he appears to fall down onto the maze
                fps_player_obj.transform.position = new Vector3(x + 0.5f, 2.0f * storey_height, z + 0.5f); 
            }
        }
        else {
            while (true) // try until a valid position is sampled
            {
                wr = Random.Range(1, width - 1);
                lr = Random.Range(1, length - 1);

                if (solution[wr, lr][0] == TileType.FLOOR)
                {
                    float x = bounds.min[0] + (float)wr * (bounds.size[0] / (float)width);
                    float z = bounds.min[2] + (float)lr * (bounds.size[2] / (float)length);
                    fps_player_obj = Instantiate(fps_prefab);
                    fps_player_obj.name = "PLAYER";
                    // character is placed above the level so that in the beginning, he appears to fall down onto the maze
                    fps_player_obj.transform.position = new Vector3(x + 0.5f, 2.0f * storey_height, z + 0.5f); 
                    break;
                }
            }
            wr_again = wr;
            lr_again = lr;
        }

        // place an exit from the maze at location (wee, lee) in terms of grid coordinates (integers)
        // destroy the wall segment there - the grid will be used to place a house
        // the exist will be placed as far as away from the character (yet, with some randomness, so that it's not always located at the corners)
        int max_dist = -1;
        int wee = -1;
        int lee = -1;
        if(wee_again != -1 && lee_again != -1) {
            wee = wee_again;
            lee = lee_again;
        }
        else {
            while (true) // try until a valid position is sampled
            {
                if (wee != -1)
                    break;
                for (int we = 0; we < width; we++)
                {
                    for (int le = 0; le < length; le++)
                    {
                        // skip corners
                        if (we == 0 && le == 0)
                            continue;
                        if (we == 0 && le == length - 1)
                            continue;
                        if (we == width - 1 && le == 0)
                            continue;
                        if (we == width - 1 && le == length - 1)
                            continue;

                        if (we == 0 || le == 0 || wee == length - 1 || lee == length - 1)
                        {
                            // randomize selection
                            if (Random.Range(0.0f, 1.0f) < 0.1f)
                            {
                                int dist = System.Math.Abs(wr - we) + System.Math.Abs(lr - le);
                                if (dist > max_dist) // must be placed far away from the player
                                {
                                    wee = we;
                                    lee = le;
                                    max_dist = dist;
                                }
                            }
                        }
                    }
                }
            }
            wee_again = wee;
            lee_again = lee;
        }

        for(int i = 0; i < width; i++) {
            for(int j = 0; j < length; j++) {
                dist[i, j] = int.MaxValue;
            }
        }

        List<int[]> frontier = new List<int[]>();
        List<int[]>[,] prev = new List<int[]>[width,length];
        frontier.Add(new int[] {wr, lr, 0});
        dist[wr, lr] = 0;

        while(frontier.Count != 0) {
            int[] node = frontier[0];
            frontier.RemoveAt(0);

            for(int i = -1; i <= 1; i++) {
                for(int j = -1; j <= 1; j++) {
                    if((i == -1 && j == -1) || (i == 1 && j == 1) || (i == 1 && j == -1) || (i == -1 && j == 1))
                        continue;
                    if((node[0]+i == wee && node[1]+j == lee) && (node[0] > 0 && node[1] > 0 && node[0] < width-1 && node[1] < length-1) && (dist[node[0]+i, node[1]+j] > dist[node[0], node[1]] && solution[node[0], node[1]][0] == TileType.WALL)) {
                        dist[node[0]+i, node[1] + j] = node[2] + 1;
                        prev[node[0]+i, node[1]+j] = new List<int[]> { node };
                        frontier.Add(new int[] {node[0] + i, node[1] + j, node[2] + 1});
                    }
                    else if((node[0]+i == wee && node[1]+j == lee) && (node[0] > 0 && node[1] > 0 && node[0] < width-1 && node[1] < length-1) && (dist[node[0]+i, node[1]+j] > dist[node[0], node[1]] && solution[node[0], node[1]][0] != TileType.WALL)) {
                        dist[node[0]+i, node[1] + j] = node[2];
                        prev[node[0]+i, node[1]+j] = new List<int[]> { node };
                        frontier.Add(new int[] {node[0] + i, node[1] + j, node[2]});
                    }
                    else if((node[0]+i > 0 && node[1]+j > 0 && node[0]+i < width-1 && node[1]+j < length-1) && dist[node[0]+i, node[1]+j] > dist[node[0], node[1]] && solution[node[0], node[1]][0] == TileType.WALL) {
                        dist[node[0]+i, node[1] + j] = node[2] + 1;
                        prev[node[0]+i, node[1]+j] = new List<int[]> { node };
                        frontier.Add(new int[] {node[0] + i, node[1] + j, node[2] + 1});
                    }
                    else if((node[0]+i > 0 && node[1]+j > 0 && node[0]+i < width-1 && node[1]+j < length-1) && dist[node[0]+i, node[1]+j] > dist[node[0], node[1]] && solution[node[0], node[1]][0] != TileType.WALL) {
                        dist[node[0]+i, node[1] + j] = node[2];
                        prev[node[0]+i, node[1]+j] = new List<int[]> { node };
                        frontier.Add(new int[] {node[0] + i, node[1] + j, node[2]});
                    }
                }
                frontier.Sort((d1, d2) => d1[2].CompareTo(d2[2]));
                frontier.Reverse();
            }
        }
        int minWallPath = dist[wee, lee];
        List<int[]> prevNode = prev[wee, lee];
        while(minWallPath != 0) {
            if(solution[prevNode[0][0], prevNode[0][1]][0] == TileType.WALL) {
                solution[prevNode[0][0], prevNode[0][1]] = new List<TileType> { TileType.FLOOR };
                minWallPath--;
            }
            if(prevNode[0][0] == wr && prevNode[0][1] == lr) break;
            prevNode = prev[prevNode[0][0], prevNode[0][1]];
        }

        for(int i = 0; i < pos_tokens.Count; i++) {
            minWallPath = dist[pos_tokens[i][0], pos_tokens[i][1]];
            prevNode = prev[pos_tokens[i][0], pos_tokens[i][1]];
            while(minWallPath != 0) {
                if(solution[prevNode[0][0], prevNode[0][1]][0] == TileType.WALL) {
                    solution[prevNode[0][0], prevNode[0][1]] = new List<TileType> { TileType.FLOOR };
                    minWallPath--;
                }
                if(prevNode[0][0] == wr && prevNode[0][1] == lr) break;
                prevNode = prev[prevNode[0][0], prevNode[0][1]];
            }
        }

        for(int i = 0; i < pos_drugs.Count; i++) {
            minWallPath = dist[pos_drugs[i][0], pos_drugs[i][1]];
            prevNode = prev[pos_drugs[i][0], pos_drugs[i][1]];
            while(minWallPath != 0) {
                if(solution[prevNode[0][0], prevNode[0][1]][0] == TileType.WALL) {
                    solution[prevNode[0][0], prevNode[0][1]] = new List<TileType> { TileType.FLOOR };
                    minWallPath--;
                }
                if(prevNode[0][0] == wr && prevNode[0][1] == lr) break;
                prevNode = prev[prevNode[0][0], prevNode[0][1]];
            }
        }

        // the rest of the code creates the scenery based on the grid state 
        // you don't need to modify this code (unless you want to replace the virus
        // or other prefabs with something else you like)
        int w = 0;
        for (float x = bounds.min[0]; x < bounds.max[0]; x += bounds.size[0] / (float)width - 1e-6f, w++)
        {
            int l = 0;
            for (float z = bounds.min[2]; z < bounds.max[2]; z += bounds.size[2] / (float)length - 1e-6f, l++)
            {
                if ((w >= width) || (l >= width))
                    continue;

                float y = bounds.min[1];
                if ((w == wee) && (l == lee)) // this is the exit
                {
                    GameObject house = Instantiate(house_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    house.name = "HOUSE";
                    house.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                    if (l == 0)
                        house.transform.Rotate(0.0f, 0.0f, 0.0f);
                    else if (w == 0)
                        house.transform.Rotate(0.0f, 90.0f, 0.0f);
                    else if (l == length - 1)
                        house.transform.Rotate(0.0f, 180.0f, 0.0f);
                    else if (w == width - 1)
                        house.transform.Rotate(0.0f, 270.0f, 0.0f);

                    house.AddComponent<BoxCollider>();
                    house.GetComponent<BoxCollider>().isTrigger = true;
                    house.GetComponent<BoxCollider>().size = new Vector3(3.0f, 3.0f, 3.0f);
                    house.AddComponent<House>();
                    obj.Add(house);
                }
                else if (solution[w, l][0] == TileType.WALL)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "WALL";
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, storey_height*2, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + 0.5f, y + storey_height / 2.0f, z + 0.5f);
                    cube.GetComponent<Renderer>().material = wallColor;
                    obj.Add(cube);
                }
                else if (solution[w, l][0] == TileType.VIRUS)
                {
                    GameObject virus = Instantiate(virus_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    virus.name = "COVID";
                    virus.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);
                    virus.AddComponent<Virus>();
                    virus.GetComponent<Rigidbody>().mass = 10000;
                    obj.Add(virus);
                }
                else if (solution[w, l][0] == TileType.DRUG)
                {
                    GameObject capsule = Instantiate(drug_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    capsule.name = "DRUG";
                    capsule.transform.position = new Vector3(x + 0.5f, y , z + 0.5f);
                    capsule.AddComponent<Drug>();
                    obj.Add(capsule);
                }
                else if (solution[w, l][0] == TileType.COIN)
                {
                    GameObject token = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    token.name = "COIN";
                    token.transform.localScale = new Vector3(1f, .1f, 1f);
                    token.transform.position = new Vector3(x + 0.5f, y + storey_height / 2.0f, z + 0.5f);
                    token.GetComponent<Renderer>().material = coinColor;
                    token.transform.Rotate(0, 0, 90);
                    token.AddComponent<Coin>();
                    obj.Add(token);
                }
            }
        }
    }

    void Update()
    {
        text_tokens.text = num_tokens_collected + " / " + num_tokens + " Tokens Retrieved";
        text_box.GetComponent<Text>().text = "Find all the tokens and reach the Castle!";
        if(num_tokens == num_tokens_collected) {
            text_box.GetComponent<Text>().text = "Reach the Castle!";
        }
    
        if (player_health < 0.001f) // the player dies here
        {
            text_box.GetComponent<Text>().text = "Failed!";

            if (fps_player_obj != null)
            {
                GameObject grave = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grave.name = "GRAVE";
                grave.transform.localScale = new Vector3(bounds.size[0] / (float)width, 2.0f * storey_height, bounds.size[2] / (float)length);
                grave.transform.position = fps_player_obj.transform.position;
                grave.GetComponent<Renderer>().material.color = Color.black;
                obj.Add(grave);
                Object.Destroy(fps_player_obj);
                cam.GetComponent<AudioListener>().enabled = true;                
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                tryAgain.SetActive(true);
            }

            return;
        }
        if (player_entered_house && num_tokens_collected == num_tokens) // the player suceeds here, variable manipulated by House.cs
        {
            Object.Destroy(fps_player_obj);
            cam.GetComponent<AudioListener>().enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PlayAgain.Active();
            return;
        }
        else if (player_entered_house && num_tokens_collected != num_tokens) {
            text_box.GetComponent<Text>().text = "Collect " + (num_tokens - num_tokens_collected) + " more token!";
            player_entered_house = false;
        }

        if(fps_player_obj.transform.position.y < -10) {
            Object.Destroy(fps_player_obj);
            TryAgain();
        }

        // virus hits the players (boolean variable is manipulated by Virus.cs)
        if (virus_landed_on_player_recently)
        {
            player_health -= Random.Range(damage_multi*0.125f, damage_multi*0.25f);
            player_health = Mathf.Max(player_health, 0.0f);
            virus_landed_on_player_recently = false;
        }

        // drug picked by the player  (boolean variable is manipulated by Drug.cs)
        if (drug_landed_on_player_recently)
        {
            player_health += Random.Range(0.25f, 0.4f);
            player_health = Mathf.Min(player_health, 1.0f);
            drug_landed_on_player_recently = false;
            virus_landed_on_player_recently = false;
        }

        // update scroll bar (not a very conventional manner to create a health bar, but whatever)
        scroll_bar.GetComponent<Scrollbar>().size = player_health;
        if (player_health < 0.5f)
        {
            ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
            cb.disabledColor = new Color(1.0f, 0.0f, 0.0f);
            scroll_bar.GetComponent<Scrollbar>().colors = cb;
        }
        else
        {
            ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
            cb.disabledColor = new Color(0.0f, 1.0f, 0.25f);
            scroll_bar.GetComponent<Scrollbar>().colors = cb;
        }
    }

    public void TryAgain() {
        tryAgain.SetActive(false);
        for(int i = 0; i < obj.Count; i++) {
            Destroy(obj[i]);
        }
        CreateGame(gridSol);
    }
}

   


    