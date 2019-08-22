var canvasElement,context,x,y,Settings,MarbleMass=[];

window.addEventListener("resize", ()=>
{
	if(canvasElement)
	{
		x=window.innerWidth;
		y=window.innerHeight;
		canvasElement.width = x;
		canvasElement.height = y;
	}
});
window.addEventListener("load", ()=>
{
	canvasElement=document.getElementById("canvasElement");
	context = canvasElement.getContext("2d");
	x=window.innerWidth;
	y=window.innerHeight;
	canvasElement.width = x;
	canvasElement.height = y;

	Settings=
	{
		BacgroundColor 	: 	"rgb(144,144,144,1)",
		MarbleCount		: 	70,
		MarbleColor 	: 	"rgb(229,0,0, 1)",
		MarbleSize		: 	5,
		MarbleSpeed		: 	1,
		LineDistanse 	: 	180,
		LineSize 		: 0.75,
		MarbleLifeTimeFrames 	: 360
	}
	Start();
});
class Marble
{
	constructor()
	{
		this.x=Math.random()*x;
		this.y=Math.random()*y;
		this.speedX=Math.random()*(Settings.MarbleSpeed*2)-Settings.MarbleSpeed;
		this.speedY=Math.random()*(Settings.MarbleSpeed*2)-Settings.MarbleSpeed;
		this.life=Math.random()*Settings.MarbleLifeTimeFrames+Settings.MarbleLifeTimeFrames;
	}
	Draw()
	{
		context.beginPath();
		context.arc(this.x,this.y,Settings.MarbleSize,0,Math.PI*2);
		context.fillStyle=Settings.MarbleColor;
		context.fill();
		context.closePath();
	}
	ReSpawn()
	{
		this.x=Math.random()*x;
		this.y=Math.random()*y;
		this.speedX=Math.random()*(Settings.MarbleSpeed*2)-Settings.MarbleSpeed;
		this.speedY=Math.random()*(Settings.MarbleSpeed*2)-Settings.MarbleSpeed;
		this.life=Math.random()*Settings.MarbleLifeTimeFrames+Settings.MarbleLifeTimeFrames;
	}
}
function Start () 
{
	for (var i = 0; i < Settings.MarbleCount; i++) 
	{
		MarbleMass[i]=new Marble();
	}
	Loop();
}
function Loop()
{
	SetBgColor();
	DrawLine();
	DrawMarble();
	requestAnimationFrame(Loop);
}
function SetBgColor () 
{
	 context.fillStyle =Settings.BacgroundColor;
	 context.fillRect(0,0,x,y);
}
function DrawMarble () 
{
	 for (var i in MarbleMass) 
	 {
	 	MarbleMass[i].x+=MarbleMass[i].speedX;
	 	MarbleMass[i].speedX=MarbleMass[i].x>x||MarbleMass[i].x<0?MarbleMass[i].speedX*=-1:MarbleMass[i].speedX;
	 	MarbleMass[i].y+=MarbleMass[i].speedY;
	 	MarbleMass[i].speedY=MarbleMass[i].y>y||MarbleMass[i].y<0?MarbleMass[i].speedY*=-1:MarbleMass[i].speedY;
	 	MarbleMass[i].Draw();
	 	MarbleMass[i].life-=1;
	 	if (MarbleMass[i].life<1) 
	 	{
	 		MarbleMass[i].ReSpawn();
	 	}
	 }
}
function DrawLine()
{
	var distanse,opst,x1,x2,y1,y2;
	 for (var a in MarbleMass) 
	 {
	 	for (var b in MarbleMass) 
	 	{
	 		if(a==b)
	 			continue;
	 		x1=MarbleMass[a].x;
	 		x2=MarbleMass[b].x;
	 		y1=MarbleMass[a].y;
	 		y2=MarbleMass[b].y;
	 		distanse=Math.sqrt(Math.pow(x1-x2,2)+Math.pow(y1-y2,2));

	 		if (distanse>Settings.LineDistanse)
	 			continue;
	 		opst=1-distanse/Settings.LineDistanse;
	 		context.beginPath();
	 		context.moveTo(x1,y1);
	 		context.lineWidth=Settings.LineSize;
	 		context.strokeStyle="rgb(0, 0, 0, "+opst+")";
	 		context.lineTo(x2,y2);
	 		context.stroke();
	 		context.closePath();
	 	}
	 }
}