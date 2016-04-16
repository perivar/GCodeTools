using System;
using System.Drawing; // Point
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// This whole algorithm is ported from the JavaScript version found at
/// http://xyzbots.com/gcode-optimizer/
/// https://github.com/andrewhodel/gcode-optimizer
/// Based on https://github.com/parano/GeneticAlgorithm-TSP
/// Ported by perivar@nerseth.com, 2016
/// </summary>
public class GAAlgorithm {
	
	private static Random rng = new Random();

	#region events that triggers on generation complete and run complete
	public delegate void GenerationCompleteHandler(GAAlgorithm sender);
	public delegate void RunCompleteHandler(GAAlgorithm sender);

	public event GenerationCompleteHandler OnGenerationComplete;
	public event RunCompleteHandler OnRunComplete;
	#endregion
	
	enum Direction {
		Next,
		Previous
	}
	
	class Best {
		public int BestPosition {get; set;}
		public int BestValue {get; set;}
		
		public override string ToString()
		{
			return string.Format("[BestPosition={0}, BestValue={1}]", BestPosition, BestValue);
		}
	}
	
	#region Private Fields
	List<Point> points = new List<Point>(); // data200
	bool running = false;
	int POPULATION_SIZE;
	double CROSSOVER_PROBABILITY;
	double MUTATION_PROBABILITY;
	
	int unchangedGenerations;

	int mutationTimes;
	int[][] distances; // distances
	int bestValue;
	List<int> best;
	int currentGeneration;

	Best currentBest;
	double[] roulette;
	List<List<int>> population;
	int[] values;
	double[] fitnessValues;
	#endregion

	#region Public Getters
	
	/// <summary>
	/// Return the current generation number
	/// </summary>
	public int CurrentGeneration {
		get {
			return currentGeneration;
		}
	}

	/// <summary>
	/// Return number of mutations that has occured so far
	/// </summary>
	public int MutationTimes {
		get {
			return mutationTimes;
		}
	}

	/// <summary>
	/// Return the best value found so far
	/// </summary>
	public double BestValue {
		get {
			return bestValue;
		}
	}

	/// <summary>
	/// Return a list of the indexes of the best path found so far
	/// </summary>
	public List<int> BestPath {
		get {
			return best;
		}
	}

	/// <summary>
	/// Returns the number of unchanged generations
	/// The higher the number, the more generations has lived without improving the path
	/// </summary>
	public int UnchangedGenerations {
		get {
			return unchangedGenerations;
		}
	}
	#endregion
	
	#region Properties
	public bool Running {
		get {
			return running;
		}
		set {
			running = value;
		}
	}
	#endregion
	
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="points">list of points</param>
	public GAAlgorithm(List<Point> points) {
		this.points = points;
		
		InitData();
		GAInitialize();
	}

	/// <summary>
	/// Main Genetic Algorithm method that mutates and calculates new value.
	/// Is to be called repeatably until the wanted result is found
	/// </summary>
	public void GANextGeneration() {
		currentGeneration++;
		Selection();
		Crossover();
		Mutation();

		SetBestValue();
	}
	
	/// <summary>
	/// Start the Genetic Algorithm and call events
	/// </summary>
	public void Run() {
		
		while (running) {
			GANextGeneration();
			if (OnGenerationComplete != null) OnGenerationComplete(this);
		}
		
		if (OnRunComplete != null) OnRunComplete(this);
	}
	
	void InitData() {
		
		running = false;

		POPULATION_SIZE = 30; 			// 30
		CROSSOVER_PROBABILITY = 0.9;  	// 0.9 or 0.7
		MUTATION_PROBABILITY  = 0.01; 	// 0.01 or 0.05

		unchangedGenerations = 0;
		mutationTimes = 0;

		bestValue = 0;
		best = new List<int>();
		currentGeneration = 0;
		currentBest = null;
		
		population = new List<List<int>>();
		values = new int[POPULATION_SIZE];
		fitnessValues = new double[POPULATION_SIZE];
		roulette = new double[POPULATION_SIZE];
	}
	
	void GAInitialize() {
		CountDistances();
		for(int i=0; i<POPULATION_SIZE; i++) {
			population.Add(RandomIndivial(points.Count));
		}
		SetBestValue();
	}
	
	void Selection() {
		const int initnum = 4;

		var parents = new List<List<int>>();
		parents.Add(population[currentBest.BestPosition]);
		parents.Add(DoMutate(best.Clone()));
		parents.Add(AddMutate(best.Clone()));
		parents.Add(best.Clone());

		SetRoulette();
		for(int i=initnum; i<POPULATION_SIZE; i++) {
			parents.Add(population[WheelOut(rng.NextDouble())]);
		}
		population = parents;
	}

	void Crossover() {
		var queue= new List<int>();
		for(int i=0; i<POPULATION_SIZE; i++) {
			if(rng.NextDouble() < CROSSOVER_PROBABILITY ) {
				queue.Add(i);
			}
		}
		queue.Shuffle();
		for(int i=0, j=queue.Count-1; i<j; i+=2) {
			DoCrossover(queue[i], queue[i+1]);
		}
	}

	void DoCrossover(int x, int y) {
		
		var child1 = GetChild(Direction.Next, x, y);
		var child2 = GetChild(Direction.Previous, x, y);
		population[x] = child1;
		population[y] = child2;
	}

	List<int> GetChild(Direction dir, int x, int y) {
		var solution = new List<int>();
		var px = population[x].Clone();
		var py = population[y].Clone();
		int dx = 0;
		int dy = 0;
		int c = px[Utils.RandomNumber(px.Count)];
		solution.Add(c);
		
		while(px.Count > 1) {
			if (dir == Direction.Next) {
				dx = px.Next(px.IndexOf(c));
				dy = py.Next(py.IndexOf(c));
			}
			if (dir == Direction.Previous) {
				dx = px.Previous(px.IndexOf(c));
				dy = py.Previous(py.IndexOf(c));
			}
			
			px.DeleteByValue(c);
			py.DeleteByValue(c);
			
			c = distances[c][dx] < distances[c][dy] ? dx : dy;
			solution.Add(c);
		}
		
		return solution;
	}

	void Mutation() {
		for(int i=0; i<POPULATION_SIZE; i++) {
			if(rng.NextDouble() < MUTATION_PROBABILITY) {
				if(rng.NextDouble() > 0.5) {
					population[i] = AddMutate(population[i]);
				} else {
					population[i] = DoMutate(population[i]);
				}
				i--;
			}
		}
	}

	List<T> DoMutate<T>(List<T> seq){
		mutationTimes++;
		int m,n = 0;
		// m and n refers to the actual index in the array
		// m range from 0 to length-2, n range from 2...Length-m
		do {
			m = Utils.RandomNumber(seq.Count - 2);
			n = Utils.RandomNumber(seq.Count);
		} while (m>=n);

		for(int i=0, j=(n-m+1)>>1; i<j; i++) {
			seq.Swap(m+i, n-i);
		}
		return seq;
	}

	List<T> AddMutate<T>(List<T> seq){
		mutationTimes++;
		int m,n = 0;
		// m and n refers to the actual index in the array
		do {
			m = Utils.RandomNumber(seq.Count>>1);
			n = Utils.RandomNumber(seq.Count);
		} while (m>=n);
		
		var s1 = seq.Slice(0,m);
		var s2 = seq.Slice(m,n);
		var s3 = seq.Slice(n,seq.Count);
		return s2.Concat(s1).Concat(s3).ToList().Clone();
	}

	void SetBestValue() {
		for(int i=0; i<population.Count; i++) {
			values[i] = Evaluate(population[i].ToArray());
		}
		currentBest = GetCurrentBest();
		if(bestValue == 0 || bestValue > currentBest.BestValue) {
			best = population[currentBest.BestPosition].Clone();
			bestValue = currentBest.BestValue;
			unchangedGenerations = 0;
		} else {
			unchangedGenerations += 1;
		}
	}

	Best GetCurrentBest() {
		int bestP = 0;
		int currentBestValue = values[0];

		for(int i=1; i<population.Count; i++) {
			if(values[i] < currentBestValue) {
				currentBestValue = values[i];
				bestP = i;
			}
		}
		return new Best() {
			BestPosition = bestP,
			BestValue = currentBestValue,
		};
	}

	void SetRoulette() {
		
		//calculate all the fitness
		for(int i=0; i<values.Length; i++) { fitnessValues[i] = 1.0/values[i]; }
		
		//set the roulette
		double sum = 0;
		for(int i=0; i<fitnessValues.Length; i++) { sum += fitnessValues[i]; }
		for(int i=0; i<roulette.Length; i++) { roulette[i] = fitnessValues[i]/sum; }
		for(int i=1; i<roulette.Length; i++) { roulette[i] += roulette[i-1]; }
	}

	int WheelOut(double rand){
		int i;
		for(i=0; i<roulette.Length; i++) {
			if( rand <= roulette[i] ) {
				return i;
			}
		}
		return 0;
	}

	/// <summary>
	/// Return a list of numbers between 0 and n that has been shuffled
	/// </summary>
	/// <param name="n">upper bound</param>
	/// <returns>a shuffled list of numbers between 0 and n</returns>
	static List<int> RandomIndivial(int n){
		var a = new List<int>();
		for(int i=0; i<n; i++) {
			a.Add(i);
		}
		a.Shuffle();
		return a;
	}

	/// <summary>
	/// Calculate the total sum of the given distances
	/// </summary>
	/// <param name="indivial">array to evaluate</param>
	/// <returns>total sum</returns>
	int Evaluate(int[] indivial) {
		int sum = distances[ indivial[0] ][ indivial[indivial.Length - 1] ];
		for(int i=1; i<indivial.Length; i++) {
			sum += distances[ indivial[i] ][ indivial[i-1] ];
		}
		return sum;
	}

	void CountDistances() {
		int length = points.Count;
		distances = new int[length][];
		for(int i=0; i<length; i++) {
			distances[i] = new int[length];
			for(int j=0; j<length; j++) {
				distances[i][j] = (int) Math.Floor(Utils.Distance(points[i], points[j]));
			}
		}
	}
	
	public override string ToString()
	{
		return string.Format("[CurrentGeneration={0}, MutationTimes={1}, BestValue={2}]", currentGeneration, mutationTimes, bestValue);
	}

	
	#region Stuff from main.js
	/*
	void drawCircle (point){
		ctx.fillStyle   = '#000';
		ctx.beginPath();
		ctx.arc(point.x, point.y, 3, 0, Math.PI*2, true);
		ctx.closePath();
		ctx.fill();
	}

	void drawLines (array){
		ctx.strokeStyle = '#f00';
		ctx.lineWidth = 1;
		ctx.beginPath();

		// move to the first point in best
		ctx.moveTo(points[array[0]].x, points[array[0]].y);

		// loop through and draw lines to each other point
		for(FIXME_VAR_TYPE i=1; i<array.Length; i++) {
			ctx.lineTo( points[array[i]].x, points[array[i]].y )
		}
		ctx.lineTo(points[array[0]].x, points[array[0]].y);

		ctx.stroke();
		ctx.closePath();
	}

	void draw() {

		if(running) {
			GANextGeneration();
			$('#status').text("There are " + points.Length + " G0 points, "
			                  +"the " + currentGeneration + "th generation with "
			                  + mutationTimes + " times of mutation. best value: "
			                  + ~~(bestValue));
		} else {
			$('#status').text("There are " + points.Length + " points")
		}

		clearCanvas();

		if (points.Length > 0) {
			// draw all the points as dots
			for(FIXME_VAR_TYPE i=0; i<points.Length; i++) {
				drawCircle(points[i]);
			}

			// draw the path
			if(best.Length == points.Length) {
				drawLines(best);
			}
		}

	}

	void clearCanvas() {
		ctx.clearRect(0, 0, WIDTH, HEIGHT);
	}
	 */
	
	/*
	$(function() {

	  	FIXME_VAR_TYPE saveAs=saveAs||function(e){"use strict";if("undefined"==typeof navigator||!/MSIE [1-9]\./.test(navigator.userAgent)){FIXME_VAR_TYPE t=e.document,n=function(){return e.URL||e.webkitURL||e},o=t.createElementNS("http://www.w3.org/1999/xhtml","a"),r="download"in o,i=function(n){FIXME_VAR_TYPE o=t.createEvent("MouseEvents");o.initMouseEvent("click",!0,!1,e,0,0,0,0,0,!1,!1,!1,!1,0,null),n.dispatchEvent(o)},a=e.webkitRequestFileSystem,c=e.requestFileSystem||a||e.mozRequestFileSystem,u=function(t){(e.setImmediate||e.setTimeout)(function(){throw t},0)},f="application/octet-stream",s=0,d=500,l=function(t){FIXME_VAR_TYPE o=function(){"string"==typeof t?n().revokeObjectURL(t):t.remove()};e.chrome?o():setTimeout(o,d)},v=function(e,t,n){t=[].concat(t);for(FIXME_VAR_TYPE o=t.Length;o--;){FIXME_VAR_TYPE r=e["on"+t[o]];if("function"==typeof r)try{r.call(e,n||e)}catch(i){u(i)}}},p=function(e){return/^\s*(?:text\/\S*|application\/xml|\S*\/\S*\+xml)\s*;.*charset\s*=\s*utf-8/i.test(e.type)?new Blob(["\ufeff",e],{type:e.type}):e},w=function(t,u){t=p(t);var d,w,y,m=this,S=t.type,h=!1,O=function(){v(m,"writestart progress write writeend".split(" "))},E=function(){if((h||!d)&&(d=n().createObjectURL(t)),w)w.location.href=d;else{FIXME_VAR_TYPE o=e.open(d,"_blank");void 0==o&&"undefined"!=typeof safari&&(e.location.href=d)}m.readyState=m.DONE,O(),l(d)},R=function(e){return function(){return m.readyState!==m.DONE?e.apply(this,arguments):void 0}},b={create:!0,exclusive:!1};return m.readyState=m.INIT,u||(u="download"),r?(d=n().createObjectURL(t),o.href=d,o.download=u,i(o),m.readyState=m.DONE,O(),void l(d)):(e.chrome&&S&&S!==f&&(y=t.slice||t.webkitSlice,t=y.call(t,0,t.size,f),h=!0),a&&"download"!==u&&(u+=".download"),(S==f||a)&&(w=e),c?(s+=t.size,void c(e.TEMPORARY,s,R(function(e){e.root.getDirectory("saved",b,R(function(e){FIXME_VAR_TYPE n=function(){e.getFile(u,b,R(function(e){e.createWriter(R(function(n){n.onwriteend=function(t){w.location.href=e.toURL(),m.readyState=m.DONE,v(m,"writeend",t),l(e)},n.onerror=function(){FIXME_VAR_TYPE e=n.error;e.code!==e.ABORT_ERR&&E()},"writestart progress write abort".split(" ").forEach(function(e){n["on"+e]=m["on"+e]}),n.write(t),m.abort=function(){n.abort(),m.readyState=m.DONE},m.readyState=m.WRITING}),E)}),E)};e.getFile(u,{create:!1},R(function(e){e.remove(),n()}),R(function(e){e.code==e.NOT_FOUND_ERR?n():E()}))}),E)}),E)):void E())},y=w.prototype,m=function(e,t){return new w(e,t)};return"undefined"!=typeof navigator&&navigator.msSaveOrOpenBlob?function(e,t){return navigator.msSaveOrOpenBlob(p(e),t)}:(y.abort=function(){FIXME_VAR_TYPE e=this;e.readyState=e.DONE,v(e,"abort")},y.readyState=y.INIT=0,y.WRITING=1,y.DONE=2,y.error=y.onwritestart=y.onprogress=y.onwrite=y.onabort=y.onerror=y.onwriteend=null,m)}}("undefined"!=typeof self&&self||"undefined"!=typeof window&&window||this.content);"undefined"!=typeof module&&module.exports?module.exports.saveAs=saveAs:"undefined"!=typeof define&&null!==define&&null!=define.amd&&define([],function(){return saveAs});

	  	init();
	  	initData();

	  	// points expects an array of objects like this
	  	// [ {x:0,y:0},{x:10,y:10} ]
	  	points = data200;

	  	FIXME_VAR_TYPE gc= document.getElementById('loadGcode');

	  	void getXY (s){
	  		FIXME_VAR_TYPE x= false;
	  		FIXME_VAR_TYPE y= false;

	  		FIXME_VAR_TYPE d= s.split(' ');

	  		for (FIXME_VAR_TYPE rr=0; rr<d.Length; rr++) {
	  			if (d[rr].substr(0,1) == 'x') {
	  				x = Number(d[rr].substr(1));
	  			} else if (d[rr].substr(0,1) == 'y') {
	  				y = Number(d[rr].substr(1));
	  			}
	  		}

	  		return [x,y];
	  	}

	  	FIXME_VAR_TYPE priorToG0= [];
	  	FIXME_VAR_TYPE eof= [];

	  	gc.addEventListener('change', function(e) {
	  	                    	FIXME_VAR_TYPE r= new FileReader();
	  	                    	r.readAsText(gc.files[0]);
	  	                    	r.onload = function(e) {

	  	                    		initData();

	  	                    		FIXME_VAR_TYPE notG0= [];
	  	                    		FIXME_VAR_TYPE allG0= [];

	  	                    		// split the file by newlines
	  	                    		FIXME_VAR_TYPE nl= r.result.split('\n');

	  	                    		console.log(nl);

	  	                    		// loop through each newline
	  	                    		for (FIXME_VAR_TYPE c=0; c<nl.Length; c++) {

	  	                    			// make everything lowercase
	  	                    			nl[c] = nl[c].toLowerCase();

	  	                    			// check if this line is a G0 command
	  	                    			if (nl[c].substr(0,3) == 'g0 ') {

	  	                    				console.log('found g0');

	  	                    				// this line is a G0 command, get the X and Y values
	  	                    				FIXME_VAR_TYPE xy= getXY(nl[c]);
	  	                    				FIXME_VAR_TYPE x= xy[0];
	  	                    				FIXME_VAR_TYPE y= xy[1];

	  	                    				// check if x or y exist for this line
	  	                    				if ((x !== false || y !== false) && (x !== false && y !== false)) {
	  	                    					// if x or y here is false we need to use the last coordinate from the previous G0 or G1 in followingLines as that is where the machine would be
	  	                    					if (y == false && allG0.Length > 0) {
	  	                    						// loop through allG0[-1].followingLines to find the most recent G0 or G1 with a y coordinate
	  	                    						for (FIXME_VAR_TYPE h=0; h<allG0[-1].followingLines.Length; h++) {
	  	                    							if ((allG0[-1].followingLines[h].substr(0,3) == 'g0 ' || allG0[-1].followingLines[h].substr(0,3) == 'g1 ') && allG0[-1].followingLines[h].match(/ y/)) {
	  	                    								// set this y coordinate as y
	  	                    								y = getXY(allG0[-1].followingLines[h])[1];
	  	                    							}
	  	                    						}
	  	                    					} else if (x == false && allG0.Length > 0) {
	  	                    						// loop through allG0[-1].followingLines to find the most recent G0 or G1 with a x coordinate
	  	                    						for (FIXME_VAR_TYPE h=0; h<allG0[-1].followingLines.Length; h++) {
	  	                    							if ((allG0[-1].followingLines[h].substr(0,3) == 'g0 ' || allG0[-1].followingLines[h].substr(0,3) == 'g1 ') && allG0[-1].followingLines[h].match(/ x/)) {
	  	                    								// set this x coordinate as x
	  	                    								x = getXY(allG0[-1].followingLines[h])[0];
	  	                    							}
	  	                    						}
	  	                    					}

	  	                    					if (allG0.Length > 0) {

	  	                    						// allG0 has entries, so we need to add notG0 to the followingLines for the previous entry in allG0
	  	                    						for (FIXME_VAR_TYPE mm=0; mm<notG0.Length; mm++) {
	  	                    							allG0[allG0.Length-1].followingLines.Add(notG0[mm]);
	  	                    						}

	  	                    					}


	  	                    					// this G0 has a valid X or Y coordinate, add it to allG0 with itself (the G0) as the first entry in followingLines
	  	                    					allG0.Add({x:x,y:y,followingLines:[nl[c]]});

	  	                    					// reset notG0
	  	                    					notG0 = [];

	  	                    				} else {
	  	                    					// there is no X or Y coordinate for this G0, we can just add it as a normal line
	  	                    					notG0.Add(nl[c]);
	  	                    				}
	  	                    			} else {
	  	                    				// add this line to notG0
	  	                    				notG0.Add(nl[c]);
	  	                    			}

	  	                    			if (allG0.Length == 0) {
	  	                    				// this holds lines prior to the first G0 for use later
	  	                    				priorToG0.Add(nl[c]);

	  	                    			}

	  	                    		}

	  	                    		console.log(notG0);

	  	                    		// add notG0 to the followingLines for the last entry in allG0
	  	                    		// this gets the lines after the last G0 in the file
	  	                    		// we also need to check if the commands here are not G0, G1, G2, G3, or G4
	  	                    		// because in this case they should be left at the end of the file, not put into the parent G0 block
	  	                    		for (FIXME_VAR_TYPE mm=0; mm<notG0.Length; mm++) {
	  	                    			FIXME_VAR_TYPE sb= notG0[mm].substr(0,3);
	  	                    			if (sb == 'g0 ' || sb == 'g1 ' || sb == 'g2 ' || sb == 'g3 ' || sb == 'g4 ') {
	  	                    				// this should be added to the parent G0 block
	  	                    				allG0[allG0.Length-1].followingLines.Add(notG0[mm]);
	  	                    			} else {
	  	                    				// this should be added to the end of the file as it was already there
	  	                    				eof.Add(notG0[mm]);
	  	                    			}
	  	                    		}

	  	                    		console.log('priorToG0',priorToG0);
	  	                    		console.log('allG0',allG0);

	  	                    		FIXME_VAR_TYPE minX= allG0[0].x;
	  	                    		FIXME_VAR_TYPE minY= allG0[0].y;
	  	                    		FIXME_VAR_TYPE maxX= allG0[0].x;
	  	                    		FIXME_VAR_TYPE maxY= allG0[0].y;

	  	                    		for (FIXME_VAR_TYPE p=0; p<allG0.Length; p++) {
	  	                    			if (allG0[p].x < minX) {
	  	                    				minX = allG0[p].x;
	  	                    			} else if (allG0[p].x > maxX) {
	  	                    				maxX = allG0[p].x;
	  	                    			}
	  	                    			if (allG0[p].y < minY) {
	  	                    				minY = allG0[p].y;
	  	                    			} else if (allG0[p].y > maxY) {
	  	                    				maxY = allG0[p].y;
	  	                    			}

	  	                    		}

	  	                    		console.log('x range: ',minX,maxX);
	  	                    		console.log('y range: ',minY,maxY);

	  	                    		// scale the points to fit the canvas 860x600

	  	                    		FIXME_VAR_TYPE xf= 860/(maxX-minX);
	  	                    		FIXME_VAR_TYPE yf= 600/(maxY-minY);

	  	                    		FIXME_VAR_TYPE sf= 1;
	  	                    		if (xf < yf) {
	  	                    			sf = xf;
	  	                    		} else {
	  	                    			sf = yf;
	  	                    		}

	  	                    		for (FIXME_VAR_TYPE p=0; p<allG0.Length; p++) {

	  	                    			// scale it
	  	                    			allG0[p].y = allG0[p].y*sf;
	  	                    			allG0[p].x = allG0[p].x*sf;

	  	                    			// flip the y axis because cnc and canvas world are opposite there
	  	                    			allG0[p].y = 600 - allG0[p].y;

	  	                    		}

	  	                    		points = allG0;
	  	                    		draw();

	  	                    		validFile = true;

	  	                    	}
	  	                    });

	  	$('#start_btn').click(function() {
	  	                      	if(points.Length >= 3) {
	  	                      		initData();
	  	                      		GAInitialize();
	  	                      		running = true;
	  	                      		ran = true;
	  	                      	} else {
	  	                      		alert("add some more points to the map!");
	  	                      	}
	  	                      });

	  	$('#save_btn').click(function() {

	  	                     	if (ran == false) {
	  	                     		alert('you must first click Start/Restart to run the optimisation before saving the file');
	  	                     		return false;
	  	                     	}

	  	                     	running = false;

	  	                     	if (validFile == false) {
	  	                     		alert('you must upload a gcode file to save an optimised version');
	  	                     		return false;
	  	                     	}

	  	                     	console.log('best',best);
	  	                     	console.log(points[best[0]]);

	  	                     	// put all the lines back together in the best order
	  	                     	FIXME_VAR_TYPE fout= '';
	  	                     	for (FIXME_VAR_TYPE c=0; c<priorToG0.Length; c++) {
	  	                     		fout += priorToG0[c] + '\n';
	  	                     	}
	  	                     	for (FIXME_VAR_TYPE c=0; c<best.Length; c++) {
	  	                     		for (FIXME_VAR_TYPE n=0; n<points[best[c]].followingLines.Length; n++) {
	  	                     			fout += points[best[c]].followingLines[n] + '\n';
	  	                     		}
	  	                     	}
	  	                     	for (FIXME_VAR_TYPE c=0; c<eof.Length; c++) {
	  	                     		fout += eof[c] + '\n';
	  	                     	}

	  	                     	FIXME_VAR_TYPE blob= new Blob([fout]);
	  	                     	FIXME_VAR_TYPE fn= gc.value;
	  	                     	if (fn.substr(0,12) == 'C:\\fakepath\\') {
	  	                     		// remove that chrome/chromium fakepath
	  	                     		fn = fn.substr(12);
	  	                     	}
	  	                     	saveAs(blob, 'optimised_'+fn, true);
	  	                     });

	  	$('#stop_btn').click(function() {
	  	                     	if(running == false && currentGeneration !== 0){
	  	                     		running = true;
	  	                     	} else {
	  	                     		running = false;
	  	                     	}

	  	                     });
	  });

	void init() {
		ctx = $('#canvas')[0].getContext("2d");
		WIDTH = $('#canvas').width();
		HEIGHT = $('#canvas').height();
		setInterval(draw, 10);
	}
	 */
	#endregion
}