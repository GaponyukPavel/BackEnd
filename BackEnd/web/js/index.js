window.addEventListener("load",()=>
		{
			if(localStorage.srch)
			{
				document.querySelector('input[name="search"]').value=localStorage.srch;
				document.getElementById("srchBY").style.display = 'inline-block';
				document.getElementById("srchBYTxt").innerText = localStorage.srch;
				
			}
			document.getElementById("cancelSearch").addEventListener("click", ()=>{
				localStorage.removeItem("srch");
				window.location.href="/index.html";
			});
			AddEvents();
			FillBooks();
		});
function AddEvents ()
{
	var mass = document.getElementsByName("main");
			for(var i=0;i<mass.length;i++)
			{
				mass[i].setAttribute('form','top-form');
				mass[i].onclick=function(evnt)
				{
					FillBooks();
				}
			}
			var mass2 = document.getElementsByName("second");
			for(var i=0;i<mass2.length;i++)
			{
				mass2[i].setAttribute('form','top-form');
				mass2[i].onclick=function()
				{
					FillBooks();
				}
			}
			var mass3 = document.getElementsByName("page");
			for(var i=0;i<mass3.length;i++)
			{
				mass3[i].setAttribute('form','top-form');
				mass3[i].onclick=function()
				{
					FillBooks();
				}
			} 
}
function FillBooks()
{
	var request =  new XMLHttpRequest();
	var main = document.querySelector("input[type=\"radio\"][name=\"main\"]:checked");
	main?main = main.value:main="Все";
	var second = document.querySelector("input[type=\"radio\"][name=\"second\"]:checked");
	second?second = second.value:second="Все";
	var page = document.querySelector("input[type=\"radio\"][name=\"page\"]:checked");
	page?page = page.value:page=1;
	var searchText = document.querySelector('input[name="search"]');
	searchText? searchText=searchText.value:searchText="";
	request.open("GET", "http://192.168.1.4/Index?main="+main+"&second="+second+"&page="+page+"&search="+searchText);
	request.addEventListener("load", ()=>
		{
			var OldBooks = document.querySelectorAll(".book_table td");
			for(var el = 0; el<OldBooks.length;el++)
			{
				OldBooks[el].innerHTML="";
			}
			var BooksMass = request.responseText.split("\n");
			BooksMass.length-=1;
			//pages bar
			var TotalPage=parseInt(BooksMass[0].split("::")[1]);
			var CurentPage=parseInt( BooksMass[0].split("::")[2]);

			var RowString="";
            for (var pg = CurentPage - 5 < 1 ? 1 : CurentPage - 5; pg < CurentPage + 5; pg++)
            {
                if (pg > TotalPage)
                    break;
                if (pg == CurentPage)
                {
                    RowString +="<input id=\""+pg+"\"name=\"page\"type=\"radio\"value=\""+pg+"\" checked=\"true\"><label for=\""+pg+"\"><a>"+pg+"</a></label>";
                    continue;
                }
                RowString += "<input id=\""+pg+"\"name=\"page\"type=\"radio\"value=\""+pg+"\"><label for=\""+pg+"\"><a>"+pg+"</a></label>";
            }
           	document.getElementById("pages").innerHTML=RowString;
           	var AllPages = document.getElementsByName("page");
         	if(AllPages)
         	{
         		for (var i = 0; i < AllPages.length; i++) 
         		{
         			AllPages[i].addEventListener("click", FillBooks);
         		}
         	}



			//books 
			for(var a=1;a<BooksMass.length;a++)
			{
				var BookElement= document.getElementById("book"+(a));
				var InnerCode="<a href=\"/book/BOOKID.html\"><div class=\"discount\"><div class=\"discount-img\">"+
					"<p class=\"DiscountAtr\">DISCOUNT</p><img src=\"/img/discount.png\"></div><img src=\"BOOKPIC\"></div><h6>AUTHOR:<br>BOOKNAME</h6>"+
					"<p>PRICE$</p></a>";
				var BooksAtr=BooksMass[a].split(":::");
				for(var b = 0;b<BooksAtr.length;b++)
				{
					var mass=BooksAtr[b].split("::");
					InnerCode=InnerCode.replace(new RegExp(mass[0],"g"),mass[1]);
				}
				BookElement.innerHTML=InnerCode;
				var DiscountAtr = BookElement.querySelector(".DiscountAtr");
				var DiscountValue = parseFloat(DiscountAtr.innerText);
				if(DiscountValue<1)
					BookElement.querySelector(".discount-img").setAttribute("hidden", "true");
				else
					DiscountAtr.innerText="-"+DiscountValue+"$";
			}
		});
		request.send();
}