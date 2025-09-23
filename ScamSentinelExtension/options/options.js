document.getElementById("saveBtn").addEventListener("click", () => {
    const apiUrl = document.getElementById("apiUrl").value.trim();
    chrome.storage.sync.set({ apiUrl }, () => {
        document.getElementById("status").innerText = "✅ API URL saved!";
    });
});

chrome.storage.sync.get("apiUrl", (data) => {
    if (data.apiUrl) {
        document.getElementById("apiUrl").value = data.apiUrl;
    }
});
