# Deploying R3E DevPack Website to Netlify

## Quick Deploy

### Option 1: Deploy with Netlify CLI

```bash
# Install Netlify CLI
npm install -g netlify-cli

# Login to Netlify
netlify login

# Deploy the website
cd /home/ubuntu/r3e-devpack-dotnet/website
netlify deploy --prod

# Follow the prompts to create a new site or link to existing
```

### Option 2: Deploy via GitHub

1. Push the website to GitHub:
```bash
cd /home/ubuntu/r3e-devpack-dotnet
git add website/ netlify.toml
git commit -m "Add R3E DevPack documentation website"
git push origin r3e
```

2. Go to [Netlify](https://app.netlify.com)
3. Click "Add new site" > "Import an existing project"
4. Connect your GitHub account and select the repository
5. Configure build settings:
   - Build command: (leave empty)
   - Publish directory: `website`
   - Branch to deploy: `r3e`
6. Click "Deploy site"

### Option 3: Drag & Drop

1. Go to [Netlify Drop](https://app.netlify.com/drop)
2. Drag the `website` folder to the browser
3. Your site will be instantly deployed

## Custom Domain Setup

After deployment, you can add a custom domain:

1. Go to your site settings in Netlify
2. Click "Domain management" > "Add custom domain"
3. Enter your domain (e.g., `docs.r3e.network`)
4. Follow DNS configuration instructions

## Environment Variables

No environment variables are required for this static site.

## Post-Deployment

After deployment:

1. **Update Download Links**: Update the download links in `downloads.html` to point to actual GitHub releases
2. **Analytics**: Add Google Analytics or similar by updating the site header
3. **Search**: Consider adding Algolia DocSearch for better search functionality
4. **CDN**: Netlify automatically provides CDN distribution

## Continuous Deployment

With GitHub integration, every push to the repository will automatically deploy updates to Netlify.

## Site Configuration

The site includes multiple configuration files:

**`netlify.toml`** (in repository root):
- Build settings
- Redirect rules
- Security headers
- Caching policies

**`_redirects`** (in website directory):
- Backup redirect rules
- 404 page handling

**`_headers`** (in website directory):
- Security headers
- Cache control

## Performance

The site is optimized for performance with:
- Minified CSS/JS (can be added in build step)
- Optimized images
- Proper caching headers
- CDN distribution

## Monitoring

Monitor your site:
- Netlify Analytics (if enabled)
- Google Analytics (add tracking code)
- Uptime monitoring services

## Support

For issues:
- Check Netlify build logs
- Review browser console for errors
- Submit issues to the GitHub repository