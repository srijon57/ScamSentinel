// --- Typing Animation for Hero Text ---
const texts = [
    "Your Anti-Scam Partner",
    "Keeping You Safe Online",
    "Detecting Fraud Instantly",
    "Protecting Your Future"
];
let textIndex = 0;
let charIndex = 0;
let isDeleting = false;
const typingElement = document.getElementById('typing-text');

function typeText() {
    const currentText = texts[textIndex];
    if (!isDeleting) {
        typingElement.textContent = currentText.substring(0, charIndex + 1);
        charIndex++;
        if (charIndex === currentText.length) {
            setTimeout(() => isDeleting = true, 2000);
        }
    } else {
        typingElement.textContent = currentText.substring(0, charIndex - 1);
        charIndex--;
        if (charIndex === 0) {
            isDeleting = false;
            textIndex = (textIndex + 1) % texts.length;
        }
    }
    setTimeout(typeText, isDeleting ? 50 : 100);
}

// --- Animated Statistics Counter ---
function animateStats() {
    const stats = [
        { id: 'stat-sites', target: 60000, suffix: '' },
        { id: 'stat-scams', target: 1367, suffix: '' },
        { id: 'stat-visitors', target: 346359, suffix: '' },
        { id: 'stat-scanned', target: 200, suffix: '' },
        { id: 'stat-reported', target: 14, suffix: '' },
        { id: 'stat-users', target: 700, suffix: '' }
    ];
    stats.forEach((stat, index) => {
        setTimeout(() => {
            let current = 0;
            const increment = stat.target / 100;
            const timer = setInterval(() => {
                current += increment;
                if (current >= stat.target) {
                    current = stat.target;
                    clearInterval(timer);
                }
                document.getElementById(stat.id).textContent =
                    Math.floor(current).toLocaleString() + stat.suffix;
            }, 20);
        }, index * 200);
    });
}

// --- Dynamic Card Generation ---
function generateScamCards() {
    const scamTypes = [
        {
            title: "Fake Supplier",
            icon: "🏭",
            color: "from-red-500 to-red-600",
            risk: "High",
            description: "Scammers pose as legitimate business suppliers to intercept payments or deliver fake goods."
        },
        {
            title: "Fake Courier",
            icon: "📦",
            color: "from-orange-500 to-orange-600",
            risk: "Medium",
            description: "Fraudsters send fake delivery notifications to steal personal info or payment details."
        },
        {
            title: "Real Estate Fraud",
            icon: "🏠",
            color: "from-purple-500 to-purple-600",
            risk: "High",
            description: "Criminals list non-existent properties or hijack listings to steal rent or deposit money."
        },
        {
            title: "Fake Products",
            icon: "📱",
            color: "from-blue-500 to-blue-600",
            risk: "Medium",
            description: "Websites or social media ads sell counterfeit or non-existent products at low prices."
        },
        {
            title: "Charity Scams",
            icon: "❤️",
            color: "from-green-500 to-green-600",
            risk: "Medium",
            description: "Scammers exploit natural disasters or global events to solicit fake donations."
        },
        {
            title: "Loan Fraud",
            icon: "💰",
            color: "from-yellow-500 to-yellow-600",
            risk: "High",
            description: "Fraudulent loan companies demand upfront fees but never provide the loan you applied for."
        },
        {
            title: "Tech Support",
            icon: "💻",
            color: "from-indigo-500 to-indigo-600",
            risk: "High",
            description: "Criminals impersonate major tech companies to gain remote access to your devices or financial accounts."
        },
        {
            title: "Romance Scams",
            icon: "💕",
            color: "from-gray-800 to-gray-900",
            risk: "High",
            description: "Con artists create fake online identities to build emotional relationships and then ask for money."
        }
    ];
    const container = document.getElementById('scam-cards');
    scamTypes.forEach((scam, index) => {
        const card = document.createElement('div');
        card.className = `card-hover bg-white rounded-2xl p-6 shadow-lg border border-gray-100 bounce-in-animation flex flex-col h-full`;
        card.style.animationDelay = `${index * 0.1}s`;
        const riskColor = scam.risk === 'High' ? 'bg-red-500' : 'bg-yellow-500';
        card.innerHTML = `
            <div class="flex flex-grow flex-col text-center">
                <div class="${scam.color} mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gradient-to-r text-2xl text-white">
                    ${scam.icon}
                </div>
                <h3 class="mb-2 text-lg font-bold text-gray-900">${scam.title}</h3>
                <div class="mb-4">
                    <span class="${riskColor} inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium text-white">
                        ${scam.risk} Risk
                    </span>
                </div>
                <p class="mb-4 flex-grow text-sm text-gray-500">
                    ${scam.description}
                </p>
            </div>
            <button class="learn-btn mt-auto w-full rounded-lg bg-gray-800 px-4 py-2 text-white transition-all duration-300 hover:bg-gray-900">
                Learn More
            </button>
        `;
        container.appendChild(card);
    });
}

// --- Search Functionality with simulated results ---
function initializeSearch() {
    const searchInput = document.getElementById('search-input');
    const searchBtn = document.getElementById('search-btn');
    const exampleTags = document.querySelectorAll('.example-tag');

    // Example tag clicks
    exampleTags.forEach(tag => {
        tag.addEventListener('click', () => {
            searchInput.value = tag.textContent;
            searchInput.focus();
        });
    });

    // Search button click
    searchBtn.addEventListener('click', performSearch);

    // Enter key search
    searchInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') performSearch();
    });

    function performSearch() {
        const query = searchInput.value.trim();
        if (!query) {
            searchInput.focus();
            return;
        }
        // Simulate search with visual feedback
        searchBtn.innerHTML = '🔄 Scanning...';
        searchBtn.disabled = true;
        setTimeout(() => {
            // Simulate results
            const isScam = Math.random() > 0.7; // 30% chance of being flagged as scam
            if (isScam) {
                showResult('⚠️ POTENTIAL SCAM DETECTED', 'This entity has been flagged by our community. Exercise extreme caution.', 'bg-red-600');
            } else {
                showResult('✅ NO THREATS FOUND', 'This entity appears to be legitimate based on our database.', 'bg-green-600');
            }
            searchBtn.innerHTML = '🔍 Scan Now';
            searchBtn.disabled = false;
        }, 2000);
    }

    // Function to display the search result pop-up
    function showResult(title, message, bgClass) {
        const result = document.createElement('div');
        result.className = `fixed top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 ${bgClass} text-white p-8 rounded-2xl shadow-2xl z-50 max-w-md w-full mx-4 animate-bounce-in`;
        result.innerHTML = `
            <div class="text-center">
                <h3 class="mb-4 text-2xl font-bold">${title}</h3>
                <p class="mb-6">${message}</p>
                <button onclick="this.parentElement.parentElement.remove()" class="rounded-lg bg-white/20 px-6 py-2 transition-colors hover:bg-white/30">
                    Close
                </button>
            </div>
        `;
        document.body.appendChild(result);
    }
}

// --- Page Initialization ---
document.addEventListener('DOMContentLoaded', () => {
    setTimeout(typeText, 1000);
    setTimeout(animateStats, 2000);
    setTimeout(generateScamCards, 1500);
    initializeSearch();
});
