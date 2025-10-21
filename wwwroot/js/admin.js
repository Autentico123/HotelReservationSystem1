// Admin Panel JavaScript

document.addEventListener('DOMContentLoaded', function () {
    // Sidebar toggle for mobile
    const sidebarToggleMobile = document.getElementById('sidebarToggleMobile');
    const sidebar = document.getElementById('sidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');
    
    if (sidebarToggleMobile) {
        sidebarToggleMobile.addEventListener('click', function () {
            sidebar.classList.toggle('active');
            
            // Create overlay if it doesn't exist
            let overlay = document.querySelector('.sidebar-overlay');
            if (!overlay) {
                overlay = document.createElement('div');
                overlay.className = 'sidebar-overlay';
                document.body.appendChild(overlay);
                
                overlay.addEventListener('click', function () {
                    sidebar.classList.remove('active');
                    overlay.classList.remove('active');
                });
            }
            overlay.classList.toggle('active');
        });
    }

    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.remove('active');
            const overlay = document.querySelector('.sidebar-overlay');
            if (overlay) {
                overlay.classList.remove('active');
            }
        });
    }

    // Close sidebar on mobile when clicking a link
    const sidebarLinks = document.querySelectorAll('.sidebar-link');
    sidebarLinks.forEach(link => {
        link.addEventListener('click', function () {
            if (window.innerWidth < 992) {
                sidebar.classList.remove('active');
                const overlay = document.querySelector('.sidebar-overlay');
                if (overlay) {
                    overlay.classList.remove('active');
                }
            }
        });
    });

    // Update current time
    function updateTime() {
        const now = new Date();
        const timeString = now.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit'
        });
        const dateString = now.toLocaleDateString('en-US', {
            month: 'short',
            day: 'numeric',
            year: 'numeric'
        });
        
        const timeElement = document.getElementById('currentTime');
        if (timeElement) {
            timeElement.textContent = `${dateString} ${timeString}`;
        }
    }

    updateTime();
    setInterval(updateTime, 1000);

    // Active link highlighting
    const currentPath = window.location.pathname;
    
    sidebarLinks.forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        }
    });

    // Auto-dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });

    // Confirm delete actions
    const deleteButtons = document.querySelectorAll('[data-confirm-delete]');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            if (!confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
                e.preventDefault();
            }
        });
    });

    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Prevent body scroll when sidebar is open on mobile
    const bodyScrollLock = {
        enable: function() {
            document.body.style.overflow = 'hidden';
        },
        disable: function() {
            document.body.style.overflow = '';
        }
    };

    // Apply scroll lock when sidebar is active on mobile
    if (sidebar) {
        const observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                if (mutation.attributeName === 'class') {
                    if (window.innerWidth < 992) {
                        if (sidebar.classList.contains('active')) {
                            bodyScrollLock.enable();
                        } else {
                            bodyScrollLock.disable();
                        }
                    }
                }
            });
        });
        
        observer.observe(sidebar, {
            attributes: true
        });
    }
});
