using System;
using Aiv.Engine;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using OpenTK;

namespace StupidAivGame
{
	public class Enemy : Character
	{
		private int timeBeforeActivation = 0;
		const int delayBeforeActivation = 500;
		private bool activated = false;
		const double MINBESTDELTA = 0.01;

		private Vector2 nextStep;
		private double lastMove = 0;
		public Enemy (string name, string formattedName, string characterName) : base (name, formattedName, characterName)
		{
		}

		public override void Start () 
		{
		}

		// TEMP
		// TODO: A* algorithm if there will ever be obstacles 
		// TODO: (futuro) algoritmo intelligente che mette in conto dove sta andando il player
		private void Follow (Player player)
		{
			// regga tangente per due punti (x - player.x) / (this.x - player.x) = (y - player.y) / (this.y - player.y)

			Vector2 playerV = new Vector2 (player.x, player.y);
			Vector2 agentV = new Vector2 (this.x, this.y);
			//List<Vector> points = new List<Vector> ();
			int distance = (int) ((playerV - agentV).Length * 2); // sucks
			double bestDelta = engine.width; // flag?
			nextStep = new Vector2();
			for (int i = 0; i <= distance; i++) {
				Vector2 newPoint = (playerV - agentV) * ((float) i / distance) + agentV;
				newPoint.X = (int) newPoint.X;
				newPoint.Y = (int) newPoint.Y;
				//if (!points.Contains(newPoint)) // sucks
				//	points.Add (newPoint);
				double pointDelta =  Math.Abs(level.speed - (newPoint - agentV).Length);
				// tries to get point closer to character's speed, usually is perfect or close to
				if (bestDelta > pointDelta) {
					bestDelta = pointDelta;
					nextStep = newPoint;
					if (bestDelta <= MINBESTDELTA) {
						break;
					}
				}
			}
			if (distance > 0) {
				//Console.WriteLine("{0} {1} {2} {3} {4}", playerV, agentV, nextStep, bestDelta, level.speed);
				this.x = (int)nextStep.X;
				this.y = (int)nextStep.Y;
			}
		}

		public override void Update () 
		{
			base.Update ();
			if (((Game) engine.objects["game"]).mainWindow == "game") {
				if (!activated) {
					if (timeBeforeActivation == 0)
						timeBeforeActivation = delayBeforeActivation;
					else {
						if (timeBeforeActivation > 0)
							timeBeforeActivation -= this.deltaTicks;
						if (timeBeforeActivation < 0) {
							activated = true;
							this.AddHitBox ("enemy_" + name, 0, 0, this.width, this.height);
						}
					}
				}
				if (activated) {
					//Shot(0);
					if (lastMove > 0)
						lastMove -= this.deltaTicks;
					if (lastMove <= 0) {
						Follow (((Game) this.engine.objects ["game"]).player);
						lastMove = 5; // move every 5ms
					}
					//Shot (1);
				}
			}
		}

	}
}

