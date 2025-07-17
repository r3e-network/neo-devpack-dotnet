# R3E Neo Contract DevPack Website

This is the official documentation website for the R3E Neo Contract DevPack toolkit.

## Features

- **Modern Design**: Clean, responsive design with dark theme
- **Comprehensive Documentation**: Getting started guides, API references, and examples
- **Downloads Page**: Direct downloads for all platform binaries
- **Interactive Elements**: Code highlighting, copy buttons, and smooth animations
- **Mobile Responsive**: Works perfectly on all devices

## Structure

```
website/
├── index.html              # Homepage
├── downloads.html          # Downloads page
├── css/
│   ├── style.css          # Main styles
│   ├── downloads.css      # Downloads page styles
│   └── docs.css           # Documentation styles
├── js/
│   ├── main.js            # Main JavaScript
│   ├── downloads.js       # Downloads page JS
│   └── docs.js            # Documentation JS
├── docs/
│   ├── getting-started.html    # Getting started guide
│   ├── compiler-reference.html # Compiler reference (TODO)
│   ├── webgui-service.html    # WebGUI documentation (TODO)
│   └── examples.html          # Examples (TODO)
├── api/
│   └── index.html         # API reference (TODO)
├── assets/                # Images and other assets
└── serve.py              # Local development server

```

## Local Development

To preview the website locally:

```bash
# Python 3
python3 serve.py

# Or using Node.js
npx http-server -p 8000

# Or using any static file server
```

Then open http://localhost:8000 in your browser.

## Deployment

This website is configured for Netlify deployment. See [DEPLOY.md](DEPLOY.md) for detailed instructions.

### Quick Deploy to Netlify

```bash
# Using Netlify CLI
netlify deploy --prod

# Or drag & drop the website folder at https://app.netlify.com/drop
```

### Other Hosting Options

- **GitHub Pages**: Push to `gh-pages` branch
- **Vercel**: Import project and deploy
- **AWS S3**: Upload files to S3 bucket with static hosting
- **Nginx**: Serve files with nginx web server

The site includes a `netlify.toml` configuration file with:
- Redirect rules
- Security headers
- Caching configuration
- Custom 404 page

## Adding Content

### New Documentation Page

1. Create HTML file in `docs/` directory
2. Use the same structure as `getting-started.html`
3. Add navigation link in sidebar
4. Include necessary CSS and JS files

### New Download

Update the download links in `downloads.html` when new releases are available.

## Technologies Used

- HTML5 & CSS3
- Vanilla JavaScript (no frameworks)
- Font Awesome icons
- Google Fonts (Inter & JetBrains Mono)
- Prism.js for syntax highlighting

## Contributing

1. Fork the repository
2. Create your feature branch
3. Make your changes
4. Test locally
5. Submit a pull request

## License

Part of the R3E Neo Contract DevPack project.