createAcctBtn = document.getElementById("createAcctBtn");
left = document.getElementById("left");
right = document.getElementById("right");
left2 = document.getElementById("left2");
right2 = document.getElementById("right2");
logo = document.querySelector(".logo");
welcomeContentChildren = document.querySelectorAll(".welcomeContent");
welcomeButtonsChildren = document.querySelectorAll(".welcomeButtons");
accountPageChildren = document.querySelectorAll(".accountPageContent");

/* This all happens when the create account button is clicked on the landing page */
createAcctBtn.addEventListener("click", function()
{
    /* Everything here up until the comment specifying the other section starting below deals with
        fading the landing page out */
    logo.classList.add("active");
    welcomeContentChildren.forEach(function(child) {
        child.classList.add("active");
    });
    welcomeButtonsChildren.forEach(function(child) {
        child.classList.add("active");
    });

    /* some things needed to be delayed to ensure smoother animation */
    setTimeout(function()
    {
        left.classList.add("active");
        right.classList.add("active");
    }, 250); /* milliseconds */

    setTimeout(function()
    {
        left.style.display = "none";
        right.style.display = "none";
        logo.style.display = "none";
        welcomeContentChildren.forEach(function(child) {
            child.style.display = "none";
        });
        welcomeButtonsChildren.forEach(function(child) {
            child.style.display = "none";
        });
    }, 1500); /* milliseconds */

    /* Now that the landing page is faded out, the create account/log in page comes into view */
    setTimeout(function()
    {
        left2.classList.add("active");
        right2.classList.add("active");
        accountPageChildren.forEach(function(child) 
        {
            child.classList.add("active");
        })
    }, 1200); /* milliseconds, this is when the create account/log in page starts fading in */
});

/* This all happens when the log in button is clicked on the landing page */
logInBtn.addEventListener("click", function()
{
    /* Everything here up until the comment specifying the other section starting below deals with
        fading the landing page out */
    logo.classList.add("active");
    welcomeContentChildren.forEach(function(child) {
        child.classList.add("active");
    });
    welcomeButtonsChildren.forEach(function(child) {
        child.classList.add("active");
    });

    /* some things needed to be delayed to ensure smoother animation */
    setTimeout(function()
    {
        left.classList.add("active");
        right.classList.add("active");
    }, 250); /* milliseconds */

    setTimeout(function()
    {
        left.style.display = "none";
        right.style.display = "none";
        logo.style.display = "none";
        welcomeContentChildren.forEach(function(child) {
            child.style.display = "none";
        });
        welcomeButtonsChildren.forEach(function(child) {
            child.style.display = "none";
        });
    }, 1500); /* milliseconds */

    /* Now that the landing page is faded out, the create account/log in page comes into view */
    setTimeout(function()
    {
        left2.classList.add("active");
        right2.classList.add("active");
        accountPageChildren.forEach(function(child) 
        {
            child.classList.add("active");
        })
    }, 1200); /* milliseconds, this is when the create account/log in page starts fading in */
});