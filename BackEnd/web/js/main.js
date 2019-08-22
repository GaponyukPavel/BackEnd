window.addEventListener("load", ()=>
	{
	 	CaclulateCart();
		
		var srchForm = document.forms["form"];
		if(srchForm)
		{
			srchForm.addEventListener("submit", ()=>
			{

				localStorage.srch= document.querySelector('input[name="search"]').value;
				window.location.href="/index.html";	
			});
			var srchFormBtn = document.getElementById("top-form-btn");
			if(srchFormBtn)
			{
				srchFormBtn.addEventListener("click", ()=>
				{
					localStorage.srch= document.querySelector('input[name="search"]').value;
				window.location.href="/index.html";	
				});
			}
		}
		
	});

function CaclulateCart()
{
	if(localStorage.BooksInCart)
	 	{
	 		var mass=localStorage.BooksInCart.split(",");
			if(mass.length>0)	
				mass.length-=1;
			var size=mass.length;
	 		document.getElementById("cartspan").innerText=size +" Товар(а)";
	 	}
	 	else
	 	{
	 		document.getElementById("cartspan").innerText="пуста";
	 	}
	if(localStorage.CartCost)
	{
		var cost = localStorage.CartCost;
		if(isNaN(cost)||cost<0)
		{
			document.getElementById("cost").innerText="0$";
		}
		document.getElementById("cost").innerText=cost+"$";
	}
	else {
		localStorage.removeItem("CartCost");
		document.getElementById("cost").innerText="0$";

	}
}
function exit()
{
	document.cookie = "LogCookie=;expires= Wed 01 Jan 1970";
}