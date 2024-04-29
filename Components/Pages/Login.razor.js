export function onLoad() {
    //console.log('Loaded');
}

export function onUpdate() {
    //console.log('Updated');
    
    document.getElementById("remember-me").onclick = function() {
        let rememberField = document.getElementById("UserCredentials.RememberMe");
        let box = document.querySelector("#remember-me .rz-chkbox-box");

        rememberField.checked = !rememberField.checked;

        if (rememberField.checked) {
            rememberField.value = true;
            box.classList.add("checked");
        } else {
            rememberField.value = false;
            box.classList.remove("checked");
        }
    };

    document.getElementById("toggle-password").onclick = function() {
        let passwordField = document.getElementById("UserCredentials.Password");
        let icon = document.querySelector("#toggle-password i");

        if (passwordField.type === "password") {
            passwordField.type = "text";
            icon.textContent = "visibility";
        } else {
            passwordField.type = "password"
            icon.textContent = "visibility_off";
        }
    };
}

export function onDispose() {
    //console.log('Disposed');
}