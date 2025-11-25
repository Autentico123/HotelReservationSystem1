// Print Report Functionality for Admin Panel

document.addEventListener('DOMContentLoaded', function () {
    setupPrintFunctionality();
});

// Print Report Functionality
function setupPrintFunctionality() {
    // Before print event
    window.addEventListener('beforeprint', function() {
        console.log('Preparing to print...');
        
        // Add any pre-print adjustments here
        document.body.classList.add('printing');
    });

    // After print event
    window.addEventListener('afterprint', function() {
        console.log('Print completed or cancelled');
        
        // Remove print class
        document.body.classList.remove('printing');
    });
}

// Custom print function with options
function printReport(title) {
    // Set document title for print
    const originalTitle = document.title;
    if (title) {
        document.title = title;
    }
    
    // Trigger print
    window.print();
    
    // Restore original title
    document.title = originalTitle;
}

// Print specific section
function printSection(sectionId) {
    const section = document.getElementById(sectionId);
    if (!section) {
        console.error('Section not found:', sectionId);
        return;
    }
    
    // Create a new window for printing
    const printWindow = window.open('', '_blank');
    
    // Get all stylesheets
    const styles = Array.from(document.querySelectorAll('link[rel="stylesheet"], style'))
        .map(style => style.outerHTML)
        .join('');
    
    // Build print content
    printWindow.document.write(`
        <!DOCTYPE html>
        <html>
        <head>
            <title>Print Report</title>
            ${styles}
        </head>
        <body>
            ${section.innerHTML}
        </body>
        </html>
    `);
    
    printWindow.document.close();
    printWindow.focus();
    
    // Wait for content to load then print
    setTimeout(() => {
        printWindow.print();
        printWindow.close();
    }, 250);
}

// Export to PDF (using browser's print to PDF)
function exportToPDF() {
    // Most modern browsers have a "Save as PDF" option in the print dialog
    alert('Please select "Save as PDF" or "Microsoft Print to PDF" from the print dialog destination/printer dropdown.');
    window.print();
}

// Print with custom date range (for reports)
function printReportWithDateRange(startDate, endDate) {
    const title = `Carmen Grand Hotel Report - ${startDate} to ${endDate}`;
    printReport(title);
}
