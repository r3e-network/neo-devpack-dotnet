// Copyright (C) 2015-2025 The Neo Project.
//
// CssTemplates.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using System;
using System.Text;

namespace Neo.Compiler.WebGui
{
    /// <summary>
    /// CSS templates for web GUI generation
    /// </summary>
    internal static class CssTemplates
    {
        public static string GetMainCss(WebGuiOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var css = new StringBuilder();

            // Base styles
            css.AppendLine(GetBaseStyles());

            // Theme styles
            if (options.DarkTheme)
                css.AppendLine(GetDarkTheme());
            else
                css.AppendLine(GetLightTheme());

            // Component styles
            css.AppendLine(GetComponentStyles());
            css.AppendLine(GetResponsiveStyles());

            // Custom CSS
            if (!string.IsNullOrEmpty(options.CustomCss))
            {
                css.AppendLine("/* Custom CSS */");
                css.AppendLine(options.CustomCss);
            }

            return css.ToString();
        }

        private static string GetBaseStyles()
        {
            return @"
/* Base Styles */
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    line-height: 1.6;
    transition: background-color 0.3s ease, color 0.3s ease;
}

.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
}

/* Header */
.header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 1rem 0;
    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

.header-content {
    max-width: 1200px;
    margin: 0 auto;
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0 20px;
}

.logo {
    display: flex;
    align-items: center;
    gap: 10px;
}

.logo i {
    font-size: 2rem;
}

.logo h1 {
    font-size: 1.8rem;
    font-weight: 600;
}

.header-actions {
    display: flex;
    gap: 10px;
}

/* Navigation Tabs */
.nav-tabs {
    display: flex;
    background: var(--card-bg);
    border-radius: 8px;
    padding: 4px;
    margin: 20px 0;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.tab-button {
    background: none;
    border: none;
    padding: 12px 20px;
    border-radius: 6px;
    cursor: pointer;
    transition: all 0.3s ease;
    color: var(--text-color);
    font-weight: 500;
}

.tab-button:hover {
    background: var(--primary-light);
    color: var(--primary-color);
}

.tab-button.active {
    background: var(--primary-color);
    color: white;
}

/* Cards */
.card {
    background: var(--card-bg);
    border-radius: 12px;
    padding: 24px;
    margin-bottom: 20px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.1);
    border: 1px solid var(--border-color);
}

.card h2 {
    margin-bottom: 20px;
    color: var(--text-color);
    display: flex;
    align-items: center;
    gap: 10px;
}

.card h3 {
    margin-bottom: 15px;
    color: var(--text-color);
    display: flex;
    align-items: center;
    gap: 8px;
}

/* Tab Content */
.tab-content {
    display: none;
}

.tab-content.active {
    display: block;
}

/* Buttons */
.btn {
    padding: 10px 16px;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 500;
    text-decoration: none;
    display: inline-flex;
    align-items: center;
    gap: 8px;
    transition: all 0.3s ease;
}

.btn-primary {
    background: var(--primary-color);
    color: white;
}

.btn-primary:hover {
    background: var(--primary-dark);
}

.btn-secondary {
    background: var(--secondary-color);
    color: white;
}

.btn-secondary:hover {
    background: var(--secondary-dark);
}

.btn-success {
    background: var(--success-color);
    color: white;
}

.btn-success:hover {
    background: var(--success-dark);
}

.btn-danger {
    background: var(--danger-color);
    color: white;
}

.btn-danger:hover {
    background: var(--danger-dark);
}

.btn-ghost {
    background: transparent;
    color: var(--text-color);
    border: 1px solid var(--border-color);
}

.btn-ghost:hover {
    background: var(--hover-bg);
}

.btn:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}

/* Forms */
input, select, textarea {
    padding: 10px;
    border: 1px solid var(--border-color);
    border-radius: 6px;
    background: var(--input-bg);
    color: var(--text-color);
    font-size: 14px;
    transition: border-color 0.3s ease;
}

input:focus, select:focus, textarea:focus {
    outline: none;
    border-color: var(--primary-color);
}

label {
    font-weight: 500;
    color: var(--text-color);
    margin-bottom: 5px;
    display: block;
}

/* Utility Classes */
.hash {
    font-family: 'Courier New', monospace;
    background: var(--code-bg);
    padding: 2px 6px;
    border-radius: 4px;
    font-size: 0.9em;
}

.loading {
    text-align: center;
    padding: 40px;
    color: var(--text-muted);
}

.error {
    background: var(--danger-light);
    color: var(--danger-color);
    padding: 12px;
    border-radius: 6px;
    margin: 10px 0;
}

.success {
    background: var(--success-light);
    color: var(--success-color);
    padding: 12px;
    border-radius: 6px;
    margin: 10px 0;
}

/* Footer */
.footer {
    background: var(--card-bg);
    border-top: 1px solid var(--border-color);
    padding: 20px 0;
    margin-top: 40px;
    text-align: center;
    color: var(--text-muted);
}
";
        }

        private static string GetLightTheme()
        {
            return @"
/* Light Theme */
:root {
    --primary-color: #667eea;
    --primary-light: #e3e8ff;
    --primary-dark: #5a6fd8;
    --secondary-color: #6c757d;
    --secondary-dark: #545b62;
    --success-color: #28a745;
    --success-light: #d4edda;
    --success-dark: #1e7e34;
    --danger-color: #dc3545;
    --danger-light: #f8d7da;
    --danger-dark: #c82333;
    --warning-color: #ffc107;
    --warning-light: #fff3cd;
    --warning-dark: #e0a800;
    
    --bg-color: #f8f9fa;
    --card-bg: #ffffff;
    --text-color: #212529;
    --text-muted: #6c757d;
    --border-color: #e9ecef;
    --hover-bg: #f8f9fa;
    --input-bg: #ffffff;
    --code-bg: #f8f9fa;
}
";
        }

        private static string GetDarkTheme()
        {
            return @"
/* Dark Theme */
.dark-theme {
    --primary-color: #667eea;
    --primary-light: #2d3748;
    --primary-dark: #5a6fd8;
    --secondary-color: #6c757d;
    --secondary-dark: #545b62;
    --success-color: #28a745;
    --success-light: #1a2e1a;
    --success-dark: #1e7e34;
    --danger-color: #dc3545;
    --danger-light: #2d1b1e;
    --danger-dark: #c82333;
    --warning-color: #ffc107;
    --warning-light: #332a1a;
    --warning-dark: #e0a800;
    
    --bg-color: #1a202c;
    --card-bg: #2d3748;
    --text-color: #f7fafc;
    --text-muted: #a0aec0;
    --border-color: #4a5568;
    --hover-bg: #2d3748;
    --input-bg: #2d3748;
    --code-bg: #1a202c;
}

.dark-theme {
    background-color: var(--bg-color);
    color: var(--text-color);
}
";
        }

        private static string GetComponentStyles()
        {
            return @"
/* Component Specific Styles */

/* Info Grid */
.info-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 20px;
}

.info-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px;
    background: var(--hover-bg);
    border-radius: 6px;
}

.info-item label {
    font-weight: 600;
    margin: 0;
}

/* Method and Event Lists */
.method-list, .event-list {
    display: flex;
    flex-direction: column;
    gap: 10px;
}

.method-item, .event-item {
    background: var(--hover-bg);
    border-radius: 8px;
    padding: 16px;
    border: 1px solid var(--border-color);
}

.method-signature, .event-signature {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 10px;
}

.method-name, .event-name {
    font-weight: 600;
    color: var(--primary-color);
}

.method-params, .event-params {
    font-family: 'Courier New', monospace;
    background: var(--code-bg);
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 0.9em;
}

.return-type {
    color: var(--success-color);
    font-weight: 500;
}

.safe-badge {
    background: var(--success-color);
    color: white;
    padding: 2px 6px;
    border-radius: 4px;
    font-size: 0.8em;
    font-weight: 500;
}

/* Method Invocation */
.method-invocation {
    display: flex;
    flex-direction: column;
    gap: 20px;
}

.method-selector {
    display: flex;
    align-items: center;
    gap: 10px;
}

.method-parameters {
    background: var(--hover-bg);
    border-radius: 8px;
    padding: 16px;
    min-height: 100px;
}

.method-actions {
    display: flex;
    gap: 10px;
}

.method-result {
    background: var(--code-bg);
    border-radius: 8px;
    padding: 16px;
    font-family: 'Courier New', monospace;
    white-space: pre-wrap;
    border: 1px solid var(--border-color);
    min-height: 100px;
}

/* Balance Monitoring */
.balance-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 20px;
    margin-bottom: 30px;
}

.balance-card {
    background: var(--hover-bg);
    border-radius: 8px;
    padding: 20px;
    text-align: center;
    border: 1px solid var(--border-color);
}

.balance-card h3 {
    margin-bottom: 10px;
    color: var(--text-muted);
    font-size: 0.9em;
    text-transform: uppercase;
    letter-spacing: 1px;
}

.balance-amount {
    font-size: 1.5em;
    font-weight: 600;
    color: var(--primary-color);
}

.balance-chart {
    background: var(--hover-bg);
    border-radius: 8px;
    padding: 20px;
}

/* Transaction History */
.transaction-filters {
    display: flex;
    gap: 10px;
    margin-bottom: 20px;
}

.transaction-filters input {
    flex: 1;
}

.transaction-list {
    max-height: 400px;
    overflow-y: auto;
}

.transaction-item {
    background: var(--hover-bg);
    border-radius: 8px;
    padding: 16px;
    margin-bottom: 10px;
    border: 1px solid var(--border-color);
}

.transaction-hash {
    font-family: 'Courier New', monospace;
    font-size: 0.9em;
    color: var(--primary-color);
}

.transaction-time {
    color: var(--text-muted);
    font-size: 0.9em;
}

.pagination {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 10px;
    margin-top: 20px;
}

/* Event Monitoring */
.event-controls {
    display: flex;
    gap: 10px;
    margin-bottom: 20px;
}

.event-list {
    max-height: 400px;
    overflow-y: auto;
}

.event-item {
    background: var(--hover-bg);
    border-radius: 8px;
    padding: 16px;
    margin-bottom: 10px;
    border-left: 4px solid var(--primary-color);
}

.event-name {
    font-weight: 600;
    color: var(--primary-color);
}

.event-data {
    font-family: 'Courier New', monospace;
    background: var(--code-bg);
    padding: 8px;
    border-radius: 4px;
    margin-top: 8px;
    font-size: 0.9em;
}

/* State Monitoring */
.state-viewer {
    display: flex;
    flex-direction: column;
    gap: 20px;
}

.state-controls {
    display: flex;
    gap: 10px;
}

.state-controls input {
    flex: 1;
}

.storage-result, .state-dump-result {
    background: var(--code-bg);
    border-radius: 8px;
    padding: 16px;
    font-family: 'Courier New', monospace;
    white-space: pre-wrap;
    border: 1px solid var(--border-color);
    min-height: 100px;
}

/* Wallet Connection */
.wallet-connection {
    display: flex;
    flex-direction: column;
    gap: 20px;
}

.wallet-status {
    display: flex;
    align-items: center;
    gap: 15px;
}

.wallet-info {
    flex: 1;
    padding: 12px;
    background: var(--hover-bg);
    border-radius: 6px;
    border: 1px solid var(--border-color);
}

.wallet-details {
    background: var(--hover-bg);
    border-radius: 8px;
    padding: 16px;
}

.wallet-detail {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 0;
    border-bottom: 1px solid var(--border-color);
}

.wallet-detail:last-child {
    border-bottom: none;
}
";
        }

        private static string GetResponsiveStyles()
        {
            return @"
/* Responsive Styles */
@media (max-width: 768px) {
    .container {
        padding: 10px;
    }
    
    .header-content {
        padding: 0 10px;
        flex-direction: column;
        gap: 15px;
    }
    
    .nav-tabs {
        flex-wrap: wrap;
        gap: 4px;
    }
    
    .tab-button {
        padding: 8px 12px;
        font-size: 0.9em;
    }
    
    .info-grid {
        grid-template-columns: 1fr;
    }
    
    .balance-grid {
        grid-template-columns: 1fr;
    }
    
    .method-selector {
        flex-direction: column;
        align-items: stretch;
    }
    
    .method-actions {
        flex-direction: column;
    }
    
    .transaction-filters {
        flex-direction: column;
    }
    
    .event-controls {
        flex-direction: column;
    }
    
    .state-controls {
        flex-direction: column;
    }
    
    .wallet-status {
        flex-direction: column;
        align-items: stretch;
    }
}

@media (max-width: 480px) {
    .card {
        padding: 16px;
    }
    
    .logo h1 {
        font-size: 1.4rem;
    }
    
    .method-signature, .event-signature {
        flex-direction: column;
        align-items: flex-start;
    }
}
";
        }
    }
}
