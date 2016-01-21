using System;
using System.Collections.Generic;
using System.Linq;
using Aiv.Engine;
using Futuridium.Characters;
using Futuridium.World;
using OpenTK;

namespace Futuridium.Game
{
    // JUST TESTING DOES NOT WORK.
    /// UnityUtils https://github.com/mortennobel/UnityUtils
    public interface IShortestPath<State, Action>
    {
        float Heuristic(State fromLocation, State toLocation);

        List<Action> Expand(State position);

        float ActualCost(State fromLocation, Action action);

        State ApplyAction(State location, Action action);
    }

    public class PriorityQueue<P, V>
    {
        private readonly SortedDictionary<P, LinkedList<V>> list = new SortedDictionary<P, LinkedList<V>>();

        public bool IsEmpty
        {
            get { return list.Count == 0; }
        }

        public void Enqueue(V value, P priority)
        {
            LinkedList<V> q;
            if (!list.TryGetValue(priority, out q))
            {
                q = new LinkedList<V>();
                list.Add(priority, q);
            }
            q.AddLast(value);
        }

        public V Dequeue()
        {
            // will throw exception if there isn’t any first element!
            var enume = list.Keys.GetEnumerator();
            enume.MoveNext();
            var key = enume.Current;
            var v = list[key];
            var res = v.First.Value;
            v.RemoveFirst();
            if (v.Count == 0)
            {
                // nothing left of the top priority.
                list.Remove(key);
            }
            return res;
        }

        public void Replace(V value, P oldPriority, P newPriority)
        {
            // TEMP WORKAROUND TEST
            if (!list.ContainsKey(oldPriority))
                return;
            var v = list[oldPriority];
            v.Remove(value);

            if (v.Count == 0)
            {
                // nothing left of the top priority.
                list.Remove(oldPriority);
            }

            Enqueue(value, newPriority);
        }

        public override string ToString()
        {
            var res = "";
            foreach (var key in list.Keys)
            {
                foreach (var val in list[key])
                {
                    res += val + ", ";
                }
            }
            return res;
        }
    }

    public class ShortestPathGraphSearch<State, Action>
    {
        private readonly IShortestPath<State, Action> info;

        public ShortestPathGraphSearch(IShortestPath<State, Action> info)
        {
            this.info = info;
        }

        public List<Action> GetShortestPath(State fromState, State toState)
        {
            var frontier = new PriorityQueue<float, SearchNode<State, Action>>();
            var exploredSet = new HashSet<State>();
            var frontierMap = new Dictionary<State, SearchNode<State, Action>>();

            var startNode = new SearchNode<State, Action>(null, 0, 0, fromState, default(Action));
            frontier.Enqueue(startNode, 0);
            frontierMap.Add(fromState, startNode);

            while (!frontier.IsEmpty)
            {
                var node = frontier.Dequeue();
                frontierMap.Remove(node.state);

                if (node.state.Equals(toState)) return BuildSolution(node);
                exploredSet.Add(node.state);
                // expand node and add to frontier
                var expand = info.Expand(node.state);
                if (expand == null) return BuildSolution(node);
                foreach (var action in expand)
                {
                    var child = info.ApplyAction(node.state, action);

                    SearchNode<State, Action> frontierNode = null;
                    var isNodeInFrontier = frontierMap.TryGetValue(child, out frontierNode);
                    if (!exploredSet.Contains(child) && !isNodeInFrontier)
                    {
                        var searchNode = CreateSearchNode(node, action, child, toState);
                        frontier.Enqueue(searchNode, searchNode.f);
                        frontierMap.Add(child, searchNode);
                    }
                    else if (isNodeInFrontier)
                    {
                        var searchNode = CreateSearchNode(node, action, child, toState);
                        if (frontierNode.f > searchNode.f)
                        {
                            frontier.Replace(frontierNode, frontierNode.f, searchNode.f);
                        }
                    }
                }
            }

            return null;
        }

        private SearchNode<State, Action> CreateSearchNode(SearchNode<State, Action> node, Action action, State child,
            State toState)
        {
            var cost = info.ActualCost(node.state, action);
            var heuristic = info.Heuristic(child, toState);
            return new SearchNode<State, Action>(node, node.g + cost, node.g + cost + heuristic, child, action);
        }

        private List<Action> BuildSolution(SearchNode<State, Action> seachNode)
        {
            var list = new List<Action>();
            while (seachNode != null)
            {
                if ((seachNode.action != null) && !seachNode.action.Equals(default(Action)))
                {
                    list.Insert(0, seachNode.action);
                }
                seachNode = seachNode.parent;
            }
            return list;
        }
    }

    public class SearchNode<State, Action> : IComparable<SearchNode<State, Action>>
    {
        public Action action;
        public float f; // estimate
        public float g; // cost
        public SearchNode<State, Action> parent;

        public State state;

        public SearchNode(SearchNode<State, Action> parent, float g, float f, State state, Action action)
        {
            this.parent = parent;
            this.g = g;
            this.f = f;
            this.state = state;
            this.action = action;
        }

        /// <summary>
        ///     Reverse sort order (smallest numbers first)
        /// </summary>
        public int CompareTo(SearchNode<State, Action> other)
        {
            return other.f.CompareTo(f);
        }

        public override string ToString()
        {
            return "SN {f:" + f + ", state: " + state + " action: " + action + "}";
        }
    }

    public class Node
    {
        public Vector2 pos { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public static class AI
    {
        public static float Minbestdelta = 5f;

        public static List<Vector2> CalculatePath(Character from, Vector2 to)
        {
            var pathNode = new PathNode(from, to);
            var search = new ShortestPathGraphSearch<Vector2, Vector2>(pathNode);
            var list = search.GetShortestPath(new Vector2(from.X, from.Y), to);
            pathNode.Dispose();
            return list;
        }
    }

    public class PathNode : IDisposable, IShortestPath<Vector2, Vector2>
    {
        private const float moveRadianStep = 1f; // 0.1f
        private readonly Vector2 objective;
        private readonly Character owner;
        private readonly GameObject testObj;

        public PathNode(Character owner, Vector2 objective)
        {
            testObj = new GameObject {Name = $"{owner.Name}_AItestObj"};
            Game.Instance.Engine.SpawnObject(testObj);
            testObj.AddHitBox("mass", (int) owner.HitBoxes["mass"].X, (int) owner.HitBoxes["mass"].Y,
                owner.HitBoxes["mass"].Width, owner.HitBoxes["mass"].Height);

            this.owner = owner;
            this.objective = objective;
        }

        public void Dispose()
        {
            testObj.Destroy();
        }

        public float Heuristic(Vector2 from, Vector2 to)
        {
            // converted to int for speed
            return (int) (from - to).LengthFast;
        }

        // TODO: rewrite with a grid structure or something more performant than this (maybe implement in aivengine?)
        public List<Vector2> Expand(Vector2 state)
        {
            var res = new List<Vector2>();
            var diff = (objective - state).Length;
            if (diff <= AI.Minbestdelta)
                return null;
            if (diff <= owner.RealSpeed)
            {
                res.Add(objective);
                return res;
            }
            // TODO: small movements
            // TODO: "swipe test" for collisions (correct)
            var roomWidth = Game.Instance.CurrentFloor.CurrentRoom.Width;
            var roomHeight = Game.Instance.CurrentFloor.CurrentRoom.Height;
            for (var r = 0f; r < Math.PI*360/180; r += moveRadianStep)
            {
                var action = new Vector2((float) Math.Cos(r), (float) Math.Sin(r));
                action.Normalize();
                action.X = action.X*owner.Level.Speed;
                action.Y = action.Y*owner.Level.Speed;
                var newState = ApplyAction(state, action);
                if (newState.X <= GameBackground.WallWidth || newState.Y <= GameBackground.WallHeight ||
                    newState.X >= roomWidth - GameBackground.WallWidth ||
                    newState.Y >= roomHeight - GameBackground.WallHeight)
                    continue;
                testObj.X = newState.X;
                testObj.Y = newState.Y;
                action.X = (int) action.X;
                action.Y = (int) action.Y;
                if (
                    !Game.Instance.CurrentFloor.CurrentRoom.GameBackground.HitBoxes.Any(
                        pair => pair.Value.CollideWith(testObj.HitBoxes["mass"])))
                    res.Add(action);
            }
            return res;
        }

        // should calculate obstacles
        public float ActualCost(Vector2 from, Vector2 to)
        {
            return Heuristic(from, to);
        }

        public Vector2 ApplyAction(Vector2 state, Vector2 action)
        {
            return state + action;
        }
    }
}