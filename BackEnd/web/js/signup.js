window.addEventListener("load",()=>
{
	var form = document.forms["aut-form"];
	form.addEventListener("submit", Check);
} );
function Check(evt)
{
	if(pass.value != pass2.value)
	{
		var style = window.getComputedStyle(error_id);
    	var top =style.getPropertyValue('height');
    	top = parseInt(top)+130;
    	error_pass.style.paddingTop=top+'px';
		error_pass.style.visibility="visible";
		evt.preventDefault();
		return;
	}
	pass.value=CryptoJS.MD5(pass.value).toString();
	//pass.setAttribute("disabled", "true");
}