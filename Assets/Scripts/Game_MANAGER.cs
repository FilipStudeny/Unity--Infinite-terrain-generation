using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Game_MANAGER : MonoBehaviour
{
    [Header("Render Distances")]
    [SerializeField] RenderDistanceLODLevels lowRange;
    [SerializeField] RenderDistanceLODLevels mediumRange;
    [SerializeField] RenderDistanceLODLevels highRange;
    [SerializeField] RenderDistanceLODLevels ultraRange;
    [SerializeField] RenderDistanceLODLevels ultraPlusPlusRange;

    [Header("Input settings")]
    [SerializeField] Slider mouseSensitivity_Slider;
    [SerializeField] Text mouseSensitivity_Text;

    [Header("Graphics settings")]
    [SerializeField] Dropdown renderDistances_DropDown;
    [SerializeField] Dropdown resolutions_Dropdown;

    [Header("Terrain UI")]
    [SerializeField] Toggle useRandomSeed_Toggle;
    [SerializeField] InputField seed_Input;
    [SerializeField] Slider terrainHeight_Slider;
    [SerializeField] Text terrainHeight_Text;
    [SerializeField] Toggle generateClouds_Toggle;
    [SerializeField] Toggle generateTerrainObjects_Toggle;
    [SerializeField] Toggle generateWater_Toggle;
    [SerializeField] Slider objectDensity_Slider;
    [SerializeField] Text objectDensity_Text;

    [Header("Noise UI")]
    [SerializeField] Slider numberOfOctaves_Slider;
    [SerializeField] Text numberOfOctaves_Text;
    [SerializeField] Slider noiseScale_Slider;
    [SerializeField] Text noiseScale_Text;
    [SerializeField] Slider persistence_Slider;
    [SerializeField] Text persistence_Text;
    [SerializeField] Slider lacunarity_Slider;
    [SerializeField] Text lacunarity_Text;
    [SerializeField] RawImage noiseTexture;

    [Header("Error UI")]
    [SerializeField] GameObject errorPanel;

    [Header("Objects")]
    [SerializeField] GameObject worldGenerator;

    TerrainData terrainData;
    Resolution[] resolutions;
    RenderDistanceLODLevels selectedRenderDistance = null;
    InfiniteTerrain_GENERATOR infiniteTerrainGENERATOR;

    private void Start()
    {
        infiniteTerrainGENERATOR = worldGenerator.GetComponent<InfiniteTerrain_GENERATOR>();

        errorPanel.SetActive(false);
        GenerateNoiseTexture();

        SetResolutions();
        renderDistances_DropDown.onValueChanged.AddListener(delegate
        {
            SetRenderDistance(renderDistances_DropDown);
        });

        mouseSensitivity_Slider.onValueChanged.AddListener(delegate
        {
            SetMouseSensitivity(mouseSensitivity_Slider);
        });

        generateWater_Toggle.onValueChanged.AddListener(delegate
        {
            RenderWater(generateWater_Toggle);
        });

        generateClouds_Toggle.onValueChanged.AddListener(delegate
        {
            RenderClouds(generateClouds_Toggle);
        });

        numberOfOctaves_Slider.onValueChanged.AddListener(delegate
        {
            GenerateNoiseTexture();
        });

        noiseScale_Slider.onValueChanged.AddListener(delegate
        {
            GenerateNoiseTexture();
        });

        persistence_Slider.onValueChanged.AddListener(delegate
        {
            GenerateNoiseTexture();
        });

        lacunarity_Slider.onValueChanged.AddListener(delegate
        {
            GenerateNoiseTexture();
        });
    }

    public void GenerateTerrain()
    {
        DestroyChildren();
        terrainData = worldGenerator.GetComponent<TerrainData>();

        terrainData.terrainSeed = useRandomSeed_Toggle.isOn ? ((int)DateTime.Now.Ticks) : int.Parse(seed_Input.text);
        terrainData.terrainHeight = (int)terrainHeight_Slider.value; 
        terrainData.generateWater = generateWater_Toggle.isOn ? true : false; 
        terrainData.generateClouds = generateClouds_Toggle.isOn ? true : false; 
        terrainData.generateObjects = generateTerrainObjects_Toggle.isOn ? true : false; 
        terrainData.objectDensity = (int)objectDensity_Slider.value; 
        terrainData.numberOfOctaves = (int)numberOfOctaves_Slider.value; 
        terrainData.noiseScale = (int)noiseScale_Slider.value; 
        terrainData.persistence = persistence_Slider.value; 
        terrainData.lacunarity = lacunarity_Slider.value; 

        worldGenerator.GetComponent<InfiniteTerrain_GENERATOR>().Generate();
    }

    void DestroyChildren()
    {
        infiniteTerrainGENERATOR.terrainChunks.Clear();
        infiniteTerrainGENERATOR.terrainChunksVisibleLastUpdate.Clear();

        while (worldGenerator.transform.childCount > 0)
        {
            DestroyImmediate(worldGenerator.transform.GetChild(0).gameObject);
        }
    }

    public void SetRenderDistance(Dropdown dropDown)
    {
        int renderDistance = dropDown.value;

        switch (renderDistance)
        {
            case 0:
                selectedRenderDistance = lowRange;
                errorPanel.SetActive(false);
                break;
            case 1:
                selectedRenderDistance = mediumRange;
                errorPanel.SetActive(false);
                break;
            case 2:
                selectedRenderDistance = highRange;
                errorPanel.SetActive(true);
                break;
            case 3:
                selectedRenderDistance = ultraRange;
                errorPanel.SetActive(true);
                break;
            case 4:
                selectedRenderDistance = ultraPlusPlusRange;
                errorPanel.SetActive(true);
                break;
        }

        worldGenerator.GetComponent<TerrainData>().renderDistance = selectedRenderDistance;
        infiniteTerrainGENERATOR.SwitchRenderDistance();
    }

 
    void SetResolutions()
    {
        resolutions = Screen.resolutions;
        resolutions_Dropdown.ClearOptions();

        List<string> availableResolutions = new List<string>();

        int currentResolution = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " x " + resolutions[i].refreshRate + "hz";
            availableResolutions.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolution = i;
            }
        }
        resolutions_Dropdown.AddOptions(availableResolutions);
        resolutions_Dropdown.value = currentResolution;
        resolutions_Dropdown.RefreshShownValue();
    }

    void GenerateNoiseTexture()
    {
        Texture2D texture = new Texture2D(240, 240);
        float[,] noise = GenerateNoise();

        for (int y = 0; y < 240; y++)
        {
            for (int x = 0; x < 240; x++)
            {
                Color color = CalculateColor(noise, x, y);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        noiseTexture.texture = texture;

    }

    Color CalculateColor(float[,] noise, int x, int y)
    {
        float sample = noise[x, y];
        return new Color(sample, sample, sample);
    }

    float[,] GenerateNoise()
    {
        int seed = useRandomSeed_Toggle.isOn ? (int)DateTime.Now.Ticks : int.Parse(seed_Input.text);
        int octaves = (int)numberOfOctaves_Slider.value;
        float persistence = persistence_Slider.value;
        float lacunarity = lacunarity_Slider.value;
        int noiseScale = (int)noiseScale_Slider.value;
        Vector2 offset = new Vector2(1f, 1f);

        NoiseData noiseMap = Noise_GENERATOR.GenerateNoise(240, octaves, seed, noiseScale, persistence, lacunarity, offset);
        return noiseMap.noiseMap;
    }

    void RenderWater(Toggle toggle)
    {
        if (toggle.isOn){
            worldGenerator.GetComponent<TerrainData>().userCamera.GetComponent<WaterCloudsFollower>().SetWaterRendering(true);
        }else{
            worldGenerator.GetComponent<TerrainData>().userCamera.GetComponent<WaterCloudsFollower>().SetWaterRendering(false);
        }
    }

    void RenderClouds(Toggle toggle)
    {
        if (toggle.isOn){
            worldGenerator.GetComponent<TerrainData>().userCamera.GetComponent<WaterCloudsFollower>().SetCloudsRendering(true);
        }else{
            worldGenerator.GetComponent<TerrainData>().userCamera.GetComponent<WaterCloudsFollower>().SetCloudsRendering(false);
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int currentResolution)
    {
        Resolution resolution = resolutions[currentResolution];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetMouseSensitivity(Slider slider)
    {
        float sensitivity = slider.value;
        terrainData.userCamera.GetComponent<CAMERA_Controller>().SetMouseSensitivity(sensitivity);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
