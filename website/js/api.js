// API Reference functionality
document.addEventListener('DOMContentLoaded', function() {
    // API Search
    const searchInput = document.getElementById('apiSearch');
    const apiItems = document.querySelectorAll('.api-item, .reference-section');
    
    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            const searchTerm = e.target.value.toLowerCase();
            
            if (searchTerm.length < 2) {
                // Show all items if search is too short
                apiItems.forEach(item => {
                    item.style.display = '';
                });
                return;
            }
            
            // Filter items based on search
            apiItems.forEach(item => {
                const text = item.textContent.toLowerCase();
                if (text.includes(searchTerm)) {
                    item.style.display = '';
                    highlightMatch(item, searchTerm);
                } else {
                    item.style.display = 'none';
                }
            });
        });
    }
    
    // Highlight matching text
    function highlightMatch(element, term) {
        // This is a simple implementation - in production, 
        // you'd want more sophisticated highlighting
        const headings = element.querySelectorAll('h3, h4');
        headings.forEach(heading => {
            const text = heading.textContent;
            const regex = new RegExp(`(${term})`, 'gi');
            if (regex.test(text)) {
                heading.classList.add('search-highlight');
                setTimeout(() => {
                    heading.classList.remove('search-highlight');
                }, 2000);
            }
        });
    }
    
    // Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
    
    // Add copy functionality to code blocks
    document.querySelectorAll('pre code').forEach((block) => {
        const button = document.createElement('button');
        button.className = 'copy-btn';
        button.innerHTML = '<i class="fas fa-copy"></i>';
        button.addEventListener('click', () => {
            navigator.clipboard.writeText(block.textContent).then(() => {
                button.innerHTML = '<i class="fas fa-check"></i>';
                setTimeout(() => {
                    button.innerHTML = '<i class="fas fa-copy"></i>';
                }, 2000);
            });
        });
        
        const pre = block.parentNode;
        pre.style.position = 'relative';
        pre.appendChild(button);
    });
    
    // API category expansion
    const categoryHeaders = document.querySelectorAll('.category-header');
    categoryHeaders.forEach(header => {
        header.style.cursor = 'pointer';
        header.addEventListener('click', () => {
            const category = header.parentElement;
            const items = category.querySelector('.api-items');
            if (items) {
                items.classList.toggle('collapsed');
                const icon = header.querySelector('i');
                if (icon) {
                    icon.style.transform = items.classList.contains('collapsed') 
                        ? 'rotate(-90deg)' 
                        : 'rotate(0)';
                }
            }
        });
    });
});

// Add search highlight styles
const searchStyles = document.createElement('style');
searchStyles.textContent = `
    .search-highlight {
        animation: highlight 0.5s ease;
        position: relative;
    }
    
    .search-highlight::after {
        content: '';
        position: absolute;
        bottom: -2px;
        left: 0;
        right: 0;
        height: 3px;
        background: var(--primary-color);
        animation: underline 0.5s ease;
    }
    
    @keyframes highlight {
        0% { color: var(--text-primary); }
        50% { color: var(--primary-color); }
        100% { color: var(--text-primary); }
    }
    
    @keyframes underline {
        0% { transform: scaleX(0); }
        100% { transform: scaleX(1); }
    }
    
    .api-items.collapsed {
        max-height: 0;
        overflow: hidden;
        transition: max-height 0.3s ease;
    }
    
    .api-items {
        max-height: 1000px;
        transition: max-height 0.3s ease;
    }
    
    .category-header i {
        transition: transform 0.3s ease;
    }
`;
document.head.appendChild(searchStyles);