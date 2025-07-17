// Documentation page functionality
function showTab(tabName) {
    const tabs = document.querySelectorAll('.tab');
    const panes = document.querySelectorAll('.tab-pane');
    
    // Remove active class from all
    tabs.forEach(tab => tab.classList.remove('active'));
    panes.forEach(pane => pane.classList.remove('active'));
    
    // Add active class to selected
    const activeTab = Array.from(tabs).find(tab => tab.textContent.toLowerCase().includes(tabName.toLowerCase()));
    if (activeTab) activeTab.classList.add('active');
    
    const activePane = document.getElementById(tabName);
    if (activePane) activePane.classList.add('active');
}

document.addEventListener('DOMContentLoaded', function() {
    // Sidebar navigation highlighting based on scroll
    const sections = document.querySelectorAll('h2[id], h1[id]');
    const navItems = document.querySelectorAll('.docs-nav .nav-item');
    
    function highlightNavigation() {
        let current = '';
        
        sections.forEach(section => {
            const sectionTop = section.offsetTop;
            const sectionHeight = section.offsetHeight;
            if (window.pageYOffset >= sectionTop - 100) {
                current = section.getAttribute('id');
            }
        });
        
        navItems.forEach(item => {
            item.classList.remove('active');
            if (item.getAttribute('href').includes(current)) {
                item.classList.add('active');
            }
        });
    }
    
    window.addEventListener('scroll', highlightNavigation);
    highlightNavigation();
    
    // Mobile sidebar toggle
    const sidebarToggle = document.createElement('button');
    sidebarToggle.className = 'sidebar-toggle';
    sidebarToggle.innerHTML = '<i class="fas fa-bars"></i>';
    sidebarToggle.addEventListener('click', () => {
        document.querySelector('.docs-sidebar').classList.toggle('active');
    });
    
    if (window.innerWidth <= 1024) {
        document.querySelector('.docs-content').prepend(sidebarToggle);
    }
    
    // Close sidebar when clicking a link on mobile
    navItems.forEach(item => {
        item.addEventListener('click', () => {
            if (window.innerWidth <= 1024) {
                document.querySelector('.docs-sidebar').classList.remove('active');
            }
        });
    });
    
    // Add anchor links to headings
    document.querySelectorAll('.docs-content h2, .docs-content h3').forEach(heading => {
        if (heading.id) {
            const anchor = document.createElement('a');
            anchor.className = 'heading-anchor';
            anchor.href = '#' + heading.id;
            anchor.innerHTML = '<i class="fas fa-link"></i>';
            heading.appendChild(anchor);
        }
    });
    
    // Smooth scroll for sidebar links
    document.querySelectorAll('.docs-nav a[href^="#"]').forEach(anchor => {
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
});

// Add documentation-specific styles
const docStyles = document.createElement('style');
docStyles.textContent = `
    .sidebar-toggle {
        position: fixed;
        top: 100px;
        left: 20px;
        z-index: 999;
        background: var(--primary-color);
        color: var(--dark-bg);
        border: none;
        border-radius: 8px;
        padding: 0.75rem;
        cursor: pointer;
        box-shadow: var(--shadow-lg);
    }
    
    .heading-anchor {
        opacity: 0;
        margin-left: 0.5rem;
        color: var(--text-secondary);
        text-decoration: none;
        transition: opacity 0.2s;
    }
    
    h2:hover .heading-anchor,
    h3:hover .heading-anchor {
        opacity: 1;
    }
    
    .heading-anchor:hover {
        color: var(--primary-color);
    }
    
    /* Syntax highlighting adjustments */
    pre[class*="language-"] {
        background: var(--card-bg) !important;
        margin: 0 !important;
    }
    
    :not(pre) > code[class*="language-"],
    pre[class*="language-"] {
        background: var(--card-bg) !important;
    }
    
    .token.comment,
    .token.prolog,
    .token.doctype,
    .token.cdata {
        color: #6c7890 !important;
    }
    
    .token.punctuation {
        color: #a0a9c9 !important;
    }
    
    .token.property,
    .token.tag,
    .token.constant,
    .token.symbol,
    .token.deleted {
        color: #ff79c6 !important;
    }
    
    .token.boolean,
    .token.number {
        color: #bd93f9 !important;
    }
    
    .token.selector,
    .token.attr-name,
    .token.string,
    .token.char,
    .token.builtin,
    .token.inserted {
        color: #50fa7b !important;
    }
    
    .token.operator,
    .token.entity,
    .token.url,
    .language-css .token.string,
    .style .token.string,
    .token.variable {
        color: #f8f8f2 !important;
    }
    
    .token.atrule,
    .token.attr-value,
    .token.function,
    .token.class-name {
        color: #f1fa8c !important;
    }
    
    .token.keyword {
        color: #8be9fd !important;
    }
    
    .token.regex,
    .token.important {
        color: #ffb86c !important;
    }
`;
document.head.appendChild(docStyles);