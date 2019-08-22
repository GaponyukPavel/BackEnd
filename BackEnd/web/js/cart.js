window.addEventListener("load", ()=>
	{
		var request = new XMLHttpRequest();
		var BooksInCart = localStorage.BooksInCart;
		if(BooksInCart&&BooksInCart.length>0)
			BooksInCart=BooksInCart.slice(0,-1);

		request.open("GET", "http://localhost/InCart?books="+BooksInCart);
		request.addEventListener("load",()=>
		{
			var ResponseValues=request.responseText.split("\n");
			AddBooksInCart(ResponseValues);
		});
		request.send();
	});
function AddBooksInCart (ResponseValues) 
{
	if(ResponseValues.length>1)
			{
				var CartValues=ResponseValues[ResponseValues.length-1].split(":::");
				localStorage.BooksInCart=CartValues[0];
				localStorage.CartCost=CartValues[1];
				CaclulateCart();

				ResponseValues.length-=1;
				document.getElementById("inner").parentNode.removeChild(document.getElementById("inner"));
				for (var a = 0; a < ResponseValues.length; a++) 
				{
					var TbodyElement = document.getElementById("cart-tbody");
					var element = document.createElement("tr");
					TbodyElement.appendChild(element);

					var InnerCode="<td><img src=\"BOOKPIC\" alt=\"Bookpicture\"><p class=\"info\">"+
					"<a href =\"/book/BOOKID.html\">BOOKNAME</a><br><span class=\"author\">AUTHOR</span>"+
					"</p><p class=\"price\"><span class=\"DefaultCost\">DEFAULTPRICE</span><span class=\"discount\" hidden>DISCOUNT</span>"+
					"<span class=\"TotalCost\">PRICE</span></p><form class =\"RemoveForm\"method=\"GET\"><input type=\"text\" hidden class=\"BookId\" value=\"BOOKID\">"+
                    "<input type=\"text\" class=\"TotalCost\" hidden value=\"TotalCost\"><a class=\"removebtn\">X</a></form></td>";

					var SplitValue=ResponseValues[a].split(":::");
					for (var b = 0; b < SplitValue.length;b++) 
					{
						var FinalValues=SplitValue[b].split("::");
						var ObjectValues={};
						ObjectValues[FinalValues[0]]=FinalValues[1];
						InnerCode=InnerCode.replace(new RegExp(FinalValues[0],"g"),FinalValues[1]);
					}
					element.innerHTML= InnerCode;
					var dis = element.getElementsByClassName("discount")[0];
					var DefaultCostElement= element.getElementsByClassName("DefaultCost")[0];
					var TotalCostElement= element.getElementsByClassName("TotalCost")[0].innerText+="$";
					if(parseFloat(dis.innerText)>0)
					{
						dis.removeAttribute("hidden");
						dis.innerText="-"+dis.innerText+"%";
						dis.innerHTML+="<br>";
						DefaultCostElement.innerText+="$";
						DefaultCostElement.innerHTML+="<br>";
					}
					else
					{
						DefaultCostElement.parentNode.removeChild(DefaultCostElement);
					}
					TotalCostElement.innerText+="$";
					var Btn=element.getElementsByClassName("removebtn")[0]; 
					Btn.addEventListener("click",()=>
						{
							RemoveButton(element.getElementsByClassName("RemoveForm")[0]);
						});
				}
			}
	else 
	{
		localStorage.BooksInCart="";
		localStorage.CartCost="";
		CaclulateCart();
	}
}
function RemoveButton(FormToSubmit)
{
	if (localStorage.BooksInCart!=undefined)
		{
			var id = FormToSubmit.getElementsByClassName("BookId")[0].value;
			if (SrchInStorage(id))
			{
				RemoveFromSorage(id);
			}
		}
	FormToSubmit.submit();
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