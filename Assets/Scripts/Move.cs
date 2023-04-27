using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Move
{
    public Coord from;
    public Coord to;

    public Coord from2;
    public Coord to2;

    public int priority;

    public Move(Coord from, Coord to, Coord from2 = null, Coord to2 = null){
        this.from = from;
        this.to = to;
        this.from2 = from2;
        this.to2 = to2;
    }

    public override string ToString(){
        return $"{from.ToString()} to {to.ToString()}";
    }
}
