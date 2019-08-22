window.addEventListener("load", ()=>{
	SetEvents();
	GetValues();
});
function GetValues()
{
	request = new XMLHttpRequest();
	request.open("GET", "http://localhost/Profile");
	request.addEventListener("load", ()=>
	{
		var userImg= document.getElementById("userPicture");
		var responseMsg = request.responseText;
		if(responseMsg)
		{
			if (responseMsg[responseMsg.length-1]==";")
				responseMsg=responseMsg.slice(0,responseMsg.length-1);
			var img = parseStrng(responseMsg,"UserPic");
			if(img)
			{
				userImg.setAttribute("src", img);
			}
			var email = parseStrng(responseMsg,"email")
			if(email)
			{
				var emailSpan = document.getElementById("email");
				if(emailSpan)
					emailSpan.innerHTML+=email;
			}
			var role = parseStrng(responseMsg,"Role")
			if(role)
			{
				var roleSpan = document.getElementById("role");
				if(roleSpan)
					roleSpan.innerHTML+=role;
			}
			var nickName = parseStrng(responseMsg,"NickName")
			if(nickName)
			{
				var nickNameSpan = document.getElementById("NickName");
				if(nickNameSpan)
					nickNameSpan.innerHTML+=nickName;
			}
		}
		else
		{

		}
	});
	request.send();
}
function SetEvents()
{
	var UserPicBtn = document.getElementById("changePicBtn");
	UserPicBtn.addEventListener("click",()=>
		{
			var element = document.createElement("div");
			element.setAttribute("id", "imgDwnld");
			//внутрянка 
			element.innerHTML='<div id="imgDwnld"><div class="uploadMenu"><form method="post" name="changePicForm" enctype="multipart/form-data"><div><label for="imageUploads" class="defBtn">Выберете изображение для загрузки(PNG, JPG)</label><input type="file" id="imageUploads" name="imageUploads" accept=".jpg, .jpeg, .png"></div><div class="preview"><p>Не выбрано ни одного файла</p></div><div class="appendButtons"><button type="button" class="defBtn">Применить</button><a class="defBtnRed">Отменить</a></div></form></div></div>';
			document.querySelector("body").insertBefore(element,document.querySelector("body").firstChild);
			document.querySelector(".appendButtons .defBtnRed").addEventListener("click", ()=>
				{
					document.querySelector("body").removeChild(element);
				});
			
			var input = document.getElementById("imageUploads");
			var form = document.forms['changePicForm'];
			document.querySelector(".appendButtons .defBtn").addEventListener("click", ()=>
				{
					if (input.files.length==1 && validFileType()&&input.files[0].size<102400)
						form.submit();
				});
			var preview = document.querySelector("div .preview");
			input.addEventListener("change", ()=>
			{
				while (preview.firstChild) 
				{
					preview.removeChild(preview.firstChild);
				}
				var paragraph = document.createElement("p");
				if(input.files.length===0)
				{
					paragraph.innerText="Не выбрано ни одного файла";
					preview.appendChild(paragraph);
					return;
				}
				var pic = input.files[0];
				if(!validFileType()||pic.size>102399)
				{
					paragraph.innerText="Файл больше 100КБ или не соответствует типу";
					preview.appendChild(paragraph);
					return;
				}

				var image = document.createElement('img');
				image.setAttribute("id", "imagePreview");
    	    	image.src = window.URL.createObjectURL(pic);
				preview.appendChild(image);
			});
			function validFileType() 
				{
					var pic2 = input.files[0];
					var fileTypes = ['image/jpeg','image/pjpeg','image/png'];
  					for(var i = 0; i < fileTypes.length; i++) 
  					{
    				if(pic2.type === fileTypes[i]) 
      					return true;
      				}
      				return false;
  				}
		});
}
//Find value (valueToFind) in string(stringToParse)
function parseStrng(stringToParse, ValueToFind)
{
	var mass = stringToParse.split(";");
	for (var i in mass) 
	{
		var values=mass[i].split("=");
		if (values[0]==ValueToFind) 
		{
			return values[1];
		}
	}
}
