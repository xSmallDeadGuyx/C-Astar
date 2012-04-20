﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

public class Pathfinder {
	public List<Vector2> FindPath(Vector2 start, Vector2 end, bool[,] area) {
		return FindPath(start, end, area, false);
	}

	public List<Vector2> FindPath(Vector2 start, Vector2 end, bool[,] area, bool cutCorners) {
		AStar finder = new AStar(start, end, area, cutCorners);
		Console.WriteLine(start);
		return finder.Generate();
	}
}

internal class AStar {
	private bool cutCorners;

	private Vector2 start;
	private Vector2 end;
	private int[,] gScore;
	private int[,] hScore;
	private int[,] fScore;
	private Vector2[,] cameFrom;
	private bool[,] walls;

	public AStar(Vector2 s, Vector2 e, bool[,] a, bool c) {
		start = s;
		end = e;
		walls = a;
		cutCorners = c;

		int w = a.GetLength(0);
		int h = a.GetLength(1);

		gScore = new int[w, h];
		hScore = new int[w, h];
		fScore = new int[w, h];
		cameFrom = new Vector2[w, h];
	}

	private int calculateHeuristic(Vector2 pos) {
		return 10 * (int) (Math.Abs(pos.X - end.X) + Math.Abs(pos.Y - end.Y));
	}

	private int distanceBetween(Vector2 pos1, Vector2 pos2) {
		return (int) Math.Round(10 * Math.Sqrt(Math.Pow(pos1.X - pos2.X, 2) + Math.Pow(pos1.Y - pos2.Y, 2)));
	}

	private Vector2 getLowestPointIn(List<Vector2> list) {
		int lowest = -1;
		Vector2 found = new Vector2(-1, -1);
		foreach(Vector2 p in list) {
			int dist = cameFrom[(int) p.X, (int) p.Y] == new Vector2(-1, -1) ? 0 : gScore[(int) cameFrom[(int) p.X, (int) p.Y].X, (int) cameFrom[(int) p.X, (int) p.Y].Y] + distanceBetween(p, cameFrom[(int) p.X, (int) p.Y]) + calculateHeuristic(p);
			if(dist <= lowest || lowest == -1) {
				lowest = dist;
				found = p;
			}
		}
		return found;
	}

	private List<Vector2> getNeighbourPoints(Vector2 p) {
		List<Vector2> found = new List<Vector2>();
		if(!walls[(int) p.X + 1, (int) p.Y]) found.Add(new Vector2((int) p.X + 1, (int) p.Y));
		if(!walls[(int) p.X - 1, (int) p.Y]) found.Add(new Vector2((int) p.X - 1, (int) p.Y));
		if(!walls[(int) p.X, (int) p.Y + 1]) found.Add(new Vector2((int) p.X, (int) p.Y + 1));
		if(!walls[(int) p.X, (int) p.Y - 1]) found.Add(new Vector2((int) p.X, (int) p.Y - 1));

		if(cutCorners) {
			if(!walls[(int) p.X + 1, (int) p.Y + 1] && (!walls[(int) p.X + 1, (int) p.Y] || !walls[(int) p.X, (int) p.Y + 1])) found.Add(new Vector2((int) p.X + 1, (int) p.Y + 1));
			if(!walls[(int) p.X - 1, (int) p.Y + 1] && (!walls[(int) p.X - 1, (int) p.Y] || !walls[(int) p.X, (int) p.Y + 1])) found.Add(new Vector2((int) p.X - 1, (int) p.Y + 1));
			if(!walls[(int) p.X - 1, (int) p.Y - 1] && (!walls[(int) p.X - 1, (int) p.Y] || !walls[(int) p.X, (int) p.Y - 1])) found.Add(new Vector2((int) p.X - 1, (int) p.Y - 1));
			if(!walls[(int) p.X + 1, (int) p.Y - 1] && (!walls[(int) p.X + 1, (int) p.Y] || !walls[(int) p.X, (int) p.Y - 1])) found.Add(new Vector2((int) p.X + 1, (int) p.Y - 1));
		}
		else {
			if(!walls[(int) p.X + 1, (int) p.Y + 1] && (!walls[(int) p.X + 1, (int) p.Y] && !walls[(int) p.X, (int) p.Y + 1])) found.Add(new Vector2((int) p.X + 1, (int) p.Y + 1));
			if(!walls[(int) p.X - 1, (int) p.Y + 1] && (!walls[(int) p.X - 1, (int) p.Y] && !walls[(int) p.X, (int) p.Y + 1])) found.Add(new Vector2((int) p.X - 1, (int) p.Y + 1));
			if(!walls[(int) p.X - 1, (int) p.Y - 1] && (!walls[(int) p.X - 1, (int) p.Y] && !walls[(int) p.X, (int) p.Y - 1])) found.Add(new Vector2((int) p.X - 1, (int) p.Y - 1));
			if(!walls[(int) p.X + 1, (int) p.Y - 1] && (!walls[(int) p.X + 1, (int) p.Y] && !walls[(int) p.X, (int) p.Y - 1])) found.Add(new Vector2((int) p.X + 1, (int) p.Y - 1));
		}
		return found;
	}

	private List<Vector2> reconstructPath(Vector2 p) {
		if(p.X > -1 && p.Y > -1) {
			List<Vector2> path = reconstructPath(cameFrom[(int) p.X, (int) p.Y]);
			path.Add(p);
			return path;
		}
		else {
			List<Vector2> path = new List<Vector2>();
			path.Add(p);
			return path;
		}
	}

	public List<Vector2> Generate() {
		List<Vector2> open = new List<Vector2>();
		List<Vector2> closed = new List<Vector2>();

		open.Add(start);
		gScore[(int) start.X, (int) start.Y] = 0;
		hScore[(int) start.X, (int) start.Y] = calculateHeuristic(start);
		fScore[(int) start.X, (int) start.Y] = hScore[(int) start.X, (int) start.Y];
		cameFrom[(int) start.X, (int) start.Y] = new Vector2(-1, -1);

		while(open.Count > 0) {
			Vector2 point = getLowestPointIn(open);
			if(point == end) return reconstructPath(point);
			open.Remove(point);
			closed.Add(point);

			List<Vector2> neighbours = getNeighbourPoints(point);
			foreach(Vector2 p in neighbours) {
				if(closed.Contains(p)) continue;

				int gPossible = gScore[(int) point.X, (int) point.Y] + distanceBetween(p, point);

				if(!open.Contains(p) || (open.Contains(p) && gPossible < gScore[(int) p.X, (int) p.Y])) {
					if(!open.Contains(p)) open.Add(p);
					cameFrom[(int) p.X, (int) p.Y] = point;
					gScore[(int) p.X, (int) p.Y] = gPossible;
					hScore[(int) p.X, (int) p.Y] = calculateHeuristic(p);
					fScore[(int) p.X, (int) p.Y] = gPossible + hScore[(int) p.X, (int) p.Y];
				}
			}
		}

		return null;
	}
}