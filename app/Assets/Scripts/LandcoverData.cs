using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // JSON DTOs for the pipeline's baked land-cover outputs (trees.json,
    // fences.json — see pipeline/terrain_pipeline/landcover.py). Positions
    // are battlefield-local meters, same frame as BattleData's DTOs.
    [Serializable]
    public class TreeDto
    {
        public float x;
        public float z;
        public string cls; // "woodlot" | "orchard"
    }

    [Serializable]
    public class TreesDto
    {
        public List<TreeDto> trees;
    }

    [Serializable]
    public class PostDto
    {
        public float x;
        public float z;
        public float bearing_deg;
        public string cls; // "stone_wall" | "rail_fence"
    }

    [Serializable]
    public class FencesDto
    {
        public List<PostDto> posts;
    }

    public static class LandcoverData
    {
        public static TreesDto ParseTrees(string json)
        {
            TreesDto dto = JsonUtility.FromJson<TreesDto>(json);
            if (dto.trees == null || dto.trees.Count == 0)
                throw new ArgumentException("trees.json has no trees");
            return dto;
        }

        public static FencesDto ParseFences(string json)
        {
            FencesDto dto = JsonUtility.FromJson<FencesDto>(json);
            if (dto.posts == null || dto.posts.Count == 0)
                throw new ArgumentException("fences.json has no posts");
            return dto;
        }
    }
}
