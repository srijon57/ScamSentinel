document.addEventListener("DOMContentLoaded", function () {
    const checkBtn = document.getElementById("checkBtn");
    const reportBtn = document.getElementById("reportBtn");
    const searchBtn = document.getElementById("searchBtn");
    const resultDiv = document.getElementById("result");
    const themeSwitch = document.getElementById("themeSwitch");

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

            // Extract malicious result
            const malicious = data?.data?.attributes?.stats?.malicious ?? 0;

            if (malicious > 0) {
                resultDiv.innerText = "⚠️ Warning: This site is flagged as malicious!";
                resultDiv.style.color = "#b91c1c";
                resultDiv.style.background = "#fef2f2";
                resultDiv.style.borderColor = "#dc2626";
            } else {
                resultDiv.innerText = "✅ Safe: This site looks safe.";
                resultDiv.style.color = "#16a34a";
                resultDiv.style.background = "#f0fdf4";
                resultDiv.style.borderColor = "#22c55e";
            }
        } catch (error) {
            console.error(error);
            resultDiv.innerText = "❌ Error checking site.";
            resultDiv.style.color = "#ea580c";
            resultDiv.style.background = "#fff7ed";
            resultDiv.style.borderColor = "#f97316";
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

    // Dark Mode Toggle
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