window.addEventListener("load",()=>
{
	var form = document.forms["aut-form"];
	form.addEventListener("submit", Check);
} );
function Check(evt)
{
	document.getElementById("error_id").innerText="";
	evt.preventDefault();
	if(pass.value != pass2.value)
	{
		var style = window.getComputedStyle(error_id);
   		var top =style.getPropertyValue('height');
   		top = parseInt(top)+130;
   		error_pass.style.paddingTop=top+'px';
		error_pass.style.visibility="visible";
	}
	else
	{
		localStorage.removeItem("BooksInCart");
		localStorage.removeItem("CartCost");
		var bodySend = "mail="+document.getElementById("mail").value+"&pass="+CryptoJS.MD5(pass.value).toString();
		var request = new XMLHttpRequest();
		request.open("POST","http://localhost/Login");
		request.onerror = function()
		{
			document.getElementById("error_id").innerText="Сервер не доступен, повторите позже";
			error_pass.style.visibility="hidden";
		}
		request.addEventListener("load", ()=>
			{
				if(request.responseText)
				{
					window.location.href="/index.html";
				}
				else
				{
					document.getElementById("error_id").innerText="Данные введены неверно!";
				}
			});
		request.send(bodySend);
	}
}