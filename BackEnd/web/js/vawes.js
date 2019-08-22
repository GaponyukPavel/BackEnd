var w,h,canvasElement,canvasContext,marbleMass=[],marbleX,marbleY,stap=1,
settings=
{
	marbleRadius			: 	15,
	distanseBetwenMarble	: 	5,
	speed 					: 	0.03,
	waveSize				: 	99

};
window.addEventListener("resize",CountMarble );
window.addEventListener("load", ()=>
	{
		canvasElement = document.getElementById("canvas");
		canvasContext = canvasElement.getContext("2d");
		CountMarble();
		loop();
	});
function CountMarble()
{
	w=window.innerWidth;
	h=window.innerHeight;
	marbleX=Math.floor((w-settings.distanseBetwenMarble)/(settings.marbleRadius*2+settings.distanseBetwenMarble));
	marbleY=Math.floor((h-settings.distanseBetwenMarble)/(settings.marbleRadius*2+settings.distanseBetwenMarble));
	var distanseX=(w-marbleX*(settings.marbleRadius*2+settings.distanseBetwenMarble))/marbleX+settings.distanseBetwenMarble;
	var distanseY=(h-marbleY*(settings.marbleRadius*2+settings.distanseBetwenMarble))/marbleY+settings.distanseBetwenMarble;
	marbleMass.length=0;
	var massNumber=0; 
	for (var a = 0 ; a < marbleX; a++) 
	{
		let tst = a*settings.marbleRadius/2;
		for (var b = 0 ; b < marbleY; b++) 
		{
			var x = a*(settings.marbleRadius*2+distanseX)+settings.marbleRadius+distanseX/2;
			var y = b*(settings.marbleRadius*2+distanseY)+settings.marbleRadius+distanseY/2;
			marbleMass[massNumber++] = new Marble(x,y);
		}
	}
}
class Marble 
{
	constructor(x,y)
	{
		this.x = x;
		this.y = y;
		this.distanse = Math.sqrt((Math.pow(w/2-x,2)+Math.pow(h/2-y,2)))/settings.waveSize;
	}
	getMarbleRadius()
	{
		return (1-Math.abs(Math.sin(this.distanse-stap)))*settings.marbleRadius;
	}
	getMarbleColor()
	{
		var red = (1-Math.abs(Math.sin(this.distanse-stap)))*255;
		return  "rgb("+red+",0,0,1)";
	}
}
function DrawMarbles()
{
	canvasElement.width = w;
	canvasElement.height = h;
	for (var i in marbleMass) 
	{
		canvasContext.beginPath();
		canvasContext.arc(marbleMass[i].x,marbleMass[i].y,marbleMass[i].getMarbleRadius(),0,Math.PI*2);
		canvasContext.fillStyle=marbleMass[i].getMarbleColor();
		canvasContext.fill();
		canvasContext.closePath();
	}
}
function loop()
{
	DrawMarbles();
	stap+=settings.speed;
	requestAnimationFrame(loop);
}