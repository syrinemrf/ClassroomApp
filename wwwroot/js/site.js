/* ===== ITBS CLASSROOM – site.js ===== */

/* ---------- DARK MODE ---------- */
function toggleDarkMode() {
    document.body.classList.toggle('dark-mode');
    const isDark = document.body.classList.contains('dark-mode');
    localStorage.setItem('darkMode', isDark ? '1' : '0');
    _updateDarkIcon(isDark);
}
function _updateDarkIcon(isDark) {
    const icon = document.getElementById('darkModeIcon');
    if (!icon) return;
    icon.classList.toggle('fa-moon', !isDark);
    icon.classList.toggle('fa-sun', isDark);
}
(function () {
    if (localStorage.getItem('darkMode') === '1') {
        document.body.classList.add('dark-mode');
        document.addEventListener('DOMContentLoaded', () => _updateDarkIcon(true));
    }
})();

/* ---------- SIDEBAR TOGGLE ---------- */
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    if (!sidebar) return;
    if (window.innerWidth <= 768) {
        sidebar.classList.toggle('show');
    } else {
        sidebar.classList.toggle('collapsed');
        const main = document.getElementById('mainContent');
        if (main) main.style.transition = 'margin-left 0.22s cubic-bezier(0.4,0,0.2,1)';
    }
}
document.addEventListener('click', function (e) {
    const sidebar = document.getElementById('sidebar');
    if (sidebar && window.innerWidth <= 768 && sidebar.classList.contains('show')) {
        if (!sidebar.contains(e.target) && !e.target.closest('.sidebar-toggle')) {
            sidebar.classList.remove('show');
        }
    }
});

/* ---------- TOASTR ---------- */
if (typeof toastr !== 'undefined') {
    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: 'toast-top-right',
        timeOut: '4000',
        newestOnTop: true,
        preventDuplicates: true,
        showMethod: 'fadeIn',
        hideMethod: 'fadeOut'
    };
}

/* ---------- DELETE CONFIRMATION ---------- */
$(document).on('submit', '.delete-form', function (e) {
    e.preventDefault();
    const form = this;
    Swal.fire({
        title: 'Confirmer la suppression',
        text: 'Cette action est irreversible.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#EF4444',
        cancelButtonColor: '#64748B',
        confirmButtonText: '<i class="fas fa-trash me-1"></i> Supprimer',
        cancelButtonText: 'Annuler',
        borderRadius: '16px',
        customClass: { popup: 'swal-modern' }
    }).then(result => { if (result.isConfirmed) form.submit(); });
});

/* ---------- ANIMATED CARD ENTRANCE ---------- */
document.addEventListener('DOMContentLoaded', function () {
    const cards = document.querySelectorAll('.card, .gc-card, .gc-stream-card');
    cards.forEach((card, i) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(12px)';
        setTimeout(() => {
            card.style.transition = 'opacity 0.35s ease, transform 0.35s ease';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, 40 + i * 35);
    });
});

/* ---------- SIGNALR NOTIFICATIONS ---------- */
$(document).ready(function () {
    if (typeof signalR !== 'undefined' && document.getElementById('notificationBell')) {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/notifications')
            .withAutomaticReconnect()
            .build();
        connection.on('ReceiveNotification', function (notification) {
            toastr.info(notification.content, notification.title);
            loadNotifications();
        });
        connection.start().then(loadNotifications).catch(err => console.warn('SignalR:', err));
    }

    function loadNotifications() {
        $.get('/Notification/GetUnreadCount', function (data) {
            const badge = $('#notificationBadge');
            if (data.count > 0) badge.text(data.count).removeClass('d-none');
            else badge.addClass('d-none');
        }).fail(() => {});

        $.get('/Notification/GetRecent', function (notifications) {
            const list = $('#notificationList');
            if (notifications && notifications.length > 0) {
                let html = '';
                notifications.forEach(n => {
                    html += `<div class="d-flex align-items-start gap-3 px-3 py-2 border-bottom">
                        <div class="stat-icon-sm mt-1"><i class="fas fa-bell text-primary small"></i></div>
                        <div class="flex-grow-1 min-w-0">
                            <div class="fw-semibold small">${n.title}</div>
                            <div class="text-muted" style="font-size:0.78rem">${n.content}</div>
                        </div>
                    </div>`;
                });
                list.html(html);
            }
        }).fail(() => {});
    }

    $('#markAllReadBtn').on('click', function () {
        const token = $('input[name="__RequestVerificationToken"]').first().val();
        $.post('/Notification/MarkAllRead', { __RequestVerificationToken: token }, loadNotifications);
    });

    /* ---------- DEADLINE COUNTDOWN ---------- */
    $('.deadline-countdown').each(function () {
        const el = $(this);
        const deadline = new Date(el.data('deadline'));
        function update() {
            const diff = deadline - new Date();
            if (diff <= 0) { el.html('<i class="fas fa-times-circle me-1"></i>Echeance depassee'); return; }
            const d = Math.floor(diff / 86400000);
            const h = Math.floor((diff % 86400000) / 3600000);
            const m = Math.floor((diff % 3600000) / 60000);
            el.html(`<i class="fas fa-clock me-1"></i>${d}j ${h}h ${m}min restants`);
        }
        update();
        setInterval(update, 60000);
    });

    /* ---------- SMOOTH TABS (GC Details) ---------- */
    $(document).on('click', '.gc-tab-link', function (e) {
        e.preventDefault();
        const href = $(this).attr('href');
        if (!href || href === '#') return;
        window.location.href = href;
    });

    /* ---------- FILE UPLOAD ZONE DRAG OVER ---------- */
    const zone = document.querySelector('.file-upload-zone');
    if (zone) {
        zone.addEventListener('dragover', (e) => { e.preventDefault(); zone.style.borderColor = 'var(--primary)'; });
        zone.addEventListener('dragleave', () => { zone.style.borderColor = ''; });
        zone.addEventListener('drop', (e) => {
            e.preventDefault(); zone.style.borderColor = '';
            const input = zone.querySelector('input[type="file"]');
            if (input && e.dataTransfer.files.length) {
                input.files = e.dataTransfer.files;
                const label = zone.querySelector('.file-zone-label');
                if (label) label.textContent = e.dataTransfer.files[0].name;
                $(input).trigger('change');
            }
        });
    }
});