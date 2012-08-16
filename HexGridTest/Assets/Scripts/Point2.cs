using UnityEngine;
using System.Collections;


/// <summary>
/// An int representation of Vector2
/// </summary>
public class Point2{

    public int x, y;

    public Point2(int _x, int _y){
        x = _x;
        y = _y;
    }
    public Point2() : this(0, 0) { }

    public int GetDistanceSquared(Point2 point) {
        int dx = this.x - point.x;
        int dy = this.y - point.y;
        return (dx * dx) + (dy * dy);
    }

    public bool EqualsSS(Point2 p) {
        return p.x == this.x && p.y == this.y;
    }

    public override int GetHashCode() {
        return (x + " " + y).GetHashCode();
    }

    public override string ToString() {
        return x + ", " + y;
    }

    public static bool operator ==(Point2 one, Point2 two) {
        return one.EqualsSS(two);
    }

    public static bool operator !=(Point2 one, Point2 two) {
        return !one.EqualsSS(two);
    }

    public static Point2 operator +(Point2 one, Point2 two) {
        return new Point2(one.x + two.x, one.y + two.y);
    }

    public static Point2 operator -(Point2 one, Point2 two) {
        return new Point2(one.x - two.x, one.y - two.y);
    }

    public static Point2 Zero = new Point2(0, 0);  
}


/// <summary>
/// An int representation of Vector3
/// </summary>
public class Point3 {

    public int x, y, z;

    public Point3(int _x, int _y, int _z) {
        x = _x;
        y = _y;
        z = _z;
    }

    /* BLAH */
}