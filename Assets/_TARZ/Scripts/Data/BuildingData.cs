using UnityEngine;

[CreateAssetMenu(
    fileName = "BuildingData",
    menuName = "TARZ/Building/Building Data"
)]
public class BuildingData : ScriptableObject
{
    public SeaVillageBuildingType buildingType;
    public GameObject prefab;
}