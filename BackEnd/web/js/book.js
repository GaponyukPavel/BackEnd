var id = /\d+/.exec(window.location.pathname)[0];
if(!id)
{
	id=1;
}
window.addEventListener("load", function()
{
	document.getElementById("AddInCart").addEventListener("click",InCart);
	
	if (localStorage.BooksInCart!=undefined)
	{
		if(SrchInStorage(id))
		{
			var element =document.getElementById("AddInCart");
			element.className="book-btn-alredy";
			element.innerText="Убрать";
		}
	}
	FillBookPage();
});

function FillBookPage()
{
	var request = new  XMLHttpRequest();
	request.open("GET", "http://localhost/BookPage?id="+id);
	request.addEventListener("load",()=>
		{
			var ResponseValues=request.responseText.split(":::");
			for (var i = 0; i < ResponseValues.length; i++) 
			{
				var Values=ResponseValues[i].split("::");
				var FindElement = document.getElementById(Values[0]);
				if(FindElement)
				{
					switch (FindElement.nodeName) 
					{
						case "IMG":
							FindElement.setAttribute("src", Values[1]);
							break;
						default:
							FindElement.innerText= Values[1];
							break;
					}
					document.getElementById("AUTHOR").addEventListener("click",()=>
						{
							localStorage.srch=document.getElementById("AUTHOR").innerText;
						}); 
				}
			}
			var dis = parseFloat(document.getElementById("DISCOUNT").innerText);
			if(!dis||dis<1)
			{
				document.getElementById("dis").setAttribute("hidden", "true");
			}
			FillLikeSection();
			FillDetails();
			FillComments();
		});
	request.send();
}

function FillComments()
{
	var request = new  XMLHttpRequest();
	request.open("GET", "http://localhost/GetComments?id="+id);
	request.addEventListener("load",()=>
	{
		var commentSection = document.getElementById("comment");
		var innerCode="";

		if(request.responseText!="")
		{
			var requestMass=request.responseText.split("::\n::");
			for (var i in requestMass) 
			{
				var mass = requestMass[i].split("::");
				var commentText=mass[0];
				var userImg=mass[1];
				var userName=mass[2];
				innerCode+="<div class=\"comment\"><div><img class=\"userpic\"src=\""+userImg+"\"alt=\"User picture\">"+
					"<p>"+userName+"</p></div><p class=\"commentText\">"+commentText+"</p></div>";
			}
		}
		commentSection.innerHTML=innerCode;
	});
	request.send();
}
function FillDetails()
{
	var detailsTable = document.getElementById("detailsTable");
	var detailsTableRow = document.getElementById("DETAILS");
	var detailsStr= detailsTableRow.innerText;
	detailsTableRow.parentNode.removeChild(detailsTableRow);
	var exampleTableRow= document.getElementById("ex");

	var detailsDate =   document.getElementById("RELEASEDATE").innerText;
	if (detailsDate)
	{
		detailsDate= detailsDate.substr(0,10);
		document.getElementById("RELEASEDATE").innerText=detailsDate;
	}
	if(detailsStr)
	{
		var detailMass = detailsStr.split("&");
		for (var i in detailMass) 
		{
			var NameValue = detailMass[i].split("=");
			var tbROW = exampleTableRow.cloneNode(true);
			tbROW.children[0].innerText=NameValue[0];
			tbROW.children[1].innerText=NameValue[1];
			tbROW.removeAttribute("id");
			detailsTable.appendChild(tbROW);

		}
	}
	exampleTableRow.parentNode.removeChild(exampleTableRow);
	detailsTable.removeAttribute("hidden");
}	

function FillLikeSection(elementsString)
{
	var request = new  XMLHttpRequest();
	request.open("GET", "http://localhost/GetLike?id="+id+"&BookName="+document.getElementById("BOOKNAME").innerText+"&Author="+document.getElementById("AUTHOR").innerText);
	request.addEventListener("load",()=>
	{
		var element = document.getElementById("GetLike");
		var elementsString=request.responseText;
		if (elementsString)
		{
			var elementsMass= elementsString.split("\n");
			for (var i in elementsMass ) 
			{
				var newElement = element.cloneNode(true);
				element.parentNode.appendChild(newElement);
				newElement.removeAttribute("id");
				var elementInner = newElement.innerHTML;
				var ms = elementsMass[i].split(":::");
				for (var a in ms) 
				{
					var nameAndValue=ms[a].split("::");
					elementInner=elementInner.replace("LIKE"+nameAndValue[0],nameAndValue[1]);
				}
				newElement.innerHTML=elementInner;
			}
		}
		element.parentNode.removeChild(element);
	});
	request.send();
}

function InCart()
	{
		var element = document.getElementById("AddInCart");
		if (element.className=="book-btn") 
		{
			if (localStorage.BooksInCart!=undefined)
			{
				var val = localStorage.BooksInCart;
				if (!SrchInStorage(id))
				{
					localStorage.BooksInCart+=id + ",";
				}
			}
			else
			{
				localStorage.BooksInCart=id + ",";
			}
			element.className="book-btn-alredy";
			element.innerText="Убрать";
			AddCost();
		}
		else
		{
			if (localStorage.BooksInCart!=undefined)
			{
				var val = localStorage.BooksInCart;
				if (SrchInStorage(id))
				{
					RemoveFromSorage(id);
				}
			}
			element.className="book-btn";
			element.innerText="В корзину";
			ReduceCost();
		}
		CaclulateCart();
	}
function AddCost()
{
	var costInCart = localStorage.CartCost;
	var cost = document.getElementById("PRICE").innerText;
	var TotalCost;
	cost=cost.replace(",",".");
	if (cost== NaN) 
		return;
	if(isNaN(costInCart)||costInCart==undefined||costInCart=="")
	{
		localStorage.CartCost=parseFloat(cost);
		return;
	}
	TotalCost=parseFloat(costInCart)+parseFloat(cost);
	TotalCost=TotalCost.toFixed(2);

	localStorage.CartCost = TotalCost;
}
function ReduceCost()
{
	if(localStorage.BooksInCart.length==0)
	{
		localStorage.CartCost=0;
		return;
	}
	var costInCart = localStorage.CartCost;
	var cost = document.getElementById("PRICE").innerText;
	cost=cost.replace(",",".");
	if (cost== NaN) 
		return;
	if(isNaN(costInCart)||costInCart==undefined||parseFloat(costInCart)- parseFloat(cost)<0)
	{
		localStorage.CartCost=0;
		return;
	}
	localStorage.CartCost = parseFloat(costInCart)- parseFloat(cost);
}
function SrchInStorage(id)
{
	var mass=localStorage.BooksInCart.split(",");
	if(mass.length>0)
		mass.length-=1;
	for (var i = 0;i<mass.length;i++)
	{
		if (id==mass[i])
			return true;
	}
	return false;
}
function RemoveFromSorage(id)
{
	var mass=localStorage.BooksInCart.split(",");
	if(mass.length>0)
		mass.length-=1;
	for (var i = 0;i<mass.length;i++)
	{
		if (id==mass[i])
		{
			mass.splice(i,1);
			var str = mass.toString();
			if (str.length>0)
				str+=",";
			localStorage.BooksInCart=str;
			return;
		}
	}
}