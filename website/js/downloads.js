// Tab functionality for installation methods
document.addEventListener('DOMContentLoaded', function() {
    const tabButtons = document.querySelectorAll('.tab-btn');
    const tabPanes = document.querySelectorAll('.tab-pane');
    
    tabButtons.forEach(button => {
        button.addEventListener('click', () => {
            const targetTab = button.getAttribute('data-tab');
            
            // Remove active class from all buttons and panes
            tabButtons.forEach(btn => btn.classList.remove('active'));
            tabPanes.forEach(pane => pane.classList.remove('active'));
            
            // Add active class to clicked button and corresponding pane
            button.classList.add('active');
            document.getElementById(targetTab).classList.add('active');
        });
    });
    
    // Detect user's OS and highlight relevant download
    const detectOS = () => {
        const platform = navigator.platform.toLowerCase();
        const userAgent = navigator.userAgent.toLowerCase();
        
        let os = 'unknown';
        if (platform.includes('win')) {
            os = 'windows';
        } else if (platform.includes('mac')) {
            if (userAgent.includes('arm64') || userAgent.includes('aarch64')) {
                os = 'macos-arm';
            } else {
                os = 'macos-intel';
            }
        } else if (platform.includes('linux')) {
            os = 'linux';
        }
        
        // Highlight the recommended download
        const cards = document.querySelectorAll('.download-card');
        cards.forEach(card => {
            const cardText = card.textContent.toLowerCase();
            if (
                (os === 'windows' && cardText.includes('windows')) ||
                (os === 'linux' && cardText.includes('linux')) ||
                (os === 'macos-intel' && cardText.includes('intel')) ||
                (os === 'macos-arm' && cardText.includes('apple silicon'))
            ) {
                card.classList.add('recommended');
                const badge = document.createElement('div');
                badge.className = 'recommended-badge';
                badge.textContent = 'Recommended for your system';
                card.insertBefore(badge, card.firstChild);
            }
        });
    };
    
    detectOS();
    
    // Fetch latest release info from GitHub API
    const fetchLatestRelease = async () => {
        try {
            const response = await fetch('https://api.github.com/repos/r3e-network/r3e-devpack-dotnet/releases/latest');
            const data = await response.json();
            
            // Update version and date
            const versionElement = document.querySelector('.version');
            const dateElement = document.querySelector('.date');
            
            if (versionElement && data.tag_name) {
                versionElement.textContent = data.tag_name;
            }
            
            if (dateElement && data.published_at) {
                const date = new Date(data.published_at);
                dateElement.textContent = `Released on ${date.toLocaleDateString('en-US', { 
                    year: 'numeric', 
                    month: 'long', 
                    day: 'numeric' 
                })}`;
            }
            
            // Update download links with actual URLs
            data.assets.forEach(asset => {
                const downloadLinks = document.querySelectorAll('.download-btn');
                downloadLinks.forEach(link => {
                    if (link.href.includes(asset.name)) {
                        link.href = asset.browser_download_url;
                    }
                });
            });
        } catch (error) {
            console.log('Could not fetch latest release info:', error);
        }
    };
    
    // Uncomment to enable fetching from GitHub API
    // fetchLatestRelease();
});

// Add styles for recommended badge
const style = document.createElement('style');
style.textContent = `
    .download-card.recommended {
        border-color: var(--primary-color);
        position: relative;
    }
    
    .recommended-badge {
        position: absolute;
        top: -12px;
        left: 50%;
        transform: translateX(-50%);
        background: var(--gradient-primary);
        color: var(--dark-bg);
        padding: 0.25rem 1rem;
        border-radius: 20px;
        font-size: 0.85rem;
        font-weight: 600;
        white-space: nowrap;
    }
`;
document.head.appendChild(style);