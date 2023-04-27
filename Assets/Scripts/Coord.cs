using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Coord
{
    [Tooltip("Horizontal")]
    public int rank;
    [Tooltip("Vertical")]
    public int file;

    public Coord(int rank, int file){
        this.rank = rank;
        this.file = file;
    }

    public override string ToString(){
        char fileLetter = (char)(rank + 65);
        return $"{fileLetter}{file + 1}";
    }
}
