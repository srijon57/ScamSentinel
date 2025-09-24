document.addEventListener("DOMContentLoaded", function () {
    const checkBtn = document.getElementById("checkBtn");
    const reportBtn = document.getElementById("reportBtn");
    const searchBtn = document.getElementById("searchBtn");
    const resultDiv = document.getElementById("result");

    // 🔍 Website check (via ScamSentinel backend proxy)
    checkBtn.addEventListener("click", () => {
        chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
            const url = tabs[0].url;
            checkWithBackend(url);
        });
    });

    async function checkWithBackend(siteUrl) {
        try {
            const response = await fetch("http://localhost:5295/api/VirusTotal/check", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(siteUrl)
            });

            if (!response.ok) {
                throw new Error("Server error: " + response.status);
            }

            const data = await response.json();
            console.log("VirusTotal Result:", data);

            // ✅ Use backend's simplified response
            if (data.isFraud) {
                resultDiv.innerText = data.message || "⚠️ Warning: This site is flagged as malicious!";
                resultDiv.style.color = "red";
                resultDiv.style.background = "#fdecea";
                resultDiv.style.borderColor = "#e74c3c";
            } else {
                resultDiv.innerText = data.message || "✅ Safe: This site looks safe.";
                resultDiv.style.color = "green";
                resultDiv.style.background = "#e9f7ef";
                resultDiv.style.borderColor = "#27ae60";
            }
        } catch (error) {
            console.error(error);
            resultDiv.innerText = "❌ Error checking site.";
            resultDiv.style.color = "orange";
            resultDiv.style.background = "#fff3e0";
            resultDiv.style.borderColor = "#f39c12";
        }
    }

    // 🚨 Report scam → opens login page
    reportBtn.addEventListener("click", () => {
        chrome.tabs.create({ url: "http://localhost:5295/Account/Login" });
    });

    // 🔎 Search fraud name → redirect to ScamList
    searchBtn.addEventListener("click", () => {
        const fraudName = document.getElementById("fraudName").value.trim();
        if (fraudName) {
            chrome.tabs.create({
                url: `http://localhost:5295/Account/ScamList?search=${encodeURIComponent(fraudName)}`
            });
        } else {
            alert("Please enter a fraud name to search.");
        }
    });
    themeSwitch.addEventListener("change", () => {
        document.body.classList.toggle("dark-mode");
        localStorage.setItem("theme", themeSwitch.checked ? "dark" : "light");
    });

    // Load saved theme
    const savedTheme = localStorage.getItem("theme");
    if (savedTheme === "dark") {
        document.body.classList.add("dark-mode");
        themeSwitch.checked = true;
    }
});
