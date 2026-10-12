const API_BASE = '/api';

// Global state cache for selects
let activeLeasesCache = [];
let roomsCache = [];
let usersCache = [];
let dormitoriesCache = [];

// DOM Loaded
document.addEventListener('DOMContentLoaded', () => {
    initApp();
});

async function initApp() {
    await loadInitialData();
    loadDashboard();
}

async function loadInitialData() {
    try {
        const [dormsRes, usersRes, roomsRes, leasesRes] = await Promise.all([
            fetch(`${API_BASE}/dormitories`),
            fetch(`${API_BASE}/users`),
            fetch(`${API_BASE}/rooms`),
            fetch(`${API_BASE}/leases/active`)
        ]);

        dormitoriesCache = await dormsRes.json();
        usersCache = await usersRes.json();
        roomsCache = await roomsRes.json();
        activeLeasesCache = await leasesRes.json();

        populateDropdowns();
    } catch (err) {
        console.error('Error loading initial data:', err);
    }
}

function populateDropdowns() {
    // Populate dormitory dropdowns
    const dormSelects = ['avail-filter-dorm', 'room-dorm-id'];
    dormSelects.forEach(id => {
        const el = document.getElementById(id);
        if (!el) return;
        const currentVal = el.value;
        const defaultText = id === 'room-dorm-id' ? '-- Select Dormitory --' : 'All Dormitories';
        el.innerHTML = `<option value="">${defaultText}</option>` +
            dormitoriesCache.map(d => `<option value="${d.dormitoryId}">${d.name}</option>`).join('');
        el.value = currentVal;
    });

    // Populate user dropdowns (Tenants)
    const userSelects = ['lease-user-id', 'reserve-user-id', 'maint-user-id'];
    userSelects.forEach(id => {
        const el = document.getElementById(id);
        if (!el) return;
        el.innerHTML = `<option value="">-- Select Person --</option>` +
            usersCache.map(u => `<option value="${u.userId}">${u.fullName} (${u.role})</option>`).join('');
    });

    // Populate room dropdowns
    const roomSelects = ['lease-room-id', 'reserve-room-id', 'maint-room-id'];
    roomSelects.forEach(id => {
        const el = document.getElementById(id);
        if (!el) return;
        el.innerHTML = `<option value="">-- Select Room --</option>` +
            roomsCache.map(r => `<option value="${r.roomId}">${r.roomNumber} - ${r.roomType} (₱${r.monthlyRate.toLocaleString()})</option>`).join('');
    });

    // Populate active lease dropdown for Paying Rent
    const payLeaseSelect = document.getElementById('pay-lease-id');
    if (payLeaseSelect) {
        payLeaseSelect.innerHTML = `<option value="">-- Choose Active Lease --</option>` +
            activeLeasesCache.map(l => `<option value="${l.leaseId}" data-rate="${l.monthlyRate}">Lease #${l.leaseId} - ${l.tenantName} (${l.roomNumber} - ₱${l.monthlyRate.toLocaleString()}/mo)</option>`).join('');
    }
}

// Tab Switching
function switchTab(tabId) {
    document.querySelectorAll('.nav-item').forEach(el => el.classList.remove('active'));
    document.querySelectorAll('.tab-section').forEach(el => el.classList.remove('active'));

    const section = document.getElementById(`tab-${tabId}`);
    if (section) section.classList.add('active');

    // Update active nav button
    const buttons = document.querySelectorAll('.nav-menu button');
    buttons.forEach(btn => {
        if (btn.getAttribute('onclick')?.includes(tabId)) {
            btn.parentElement.classList.add('active');
        }
    });

    const headings = {
        'dashboard': 'Dashboard Overview',
        'available-rooms': 'Available Rooms & Vacancies',
        'all-rooms': 'Room Management',
        'payments': 'Rent Payment & Financial History',
        'leases': 'Active Leases & Tenant Check-Ins',
        'reservations': 'Room Reservations',
        'maintenance': 'Maintenance & Repair Requests',
        'dormitories': 'Dormitory Buildings & Capacity',
        'tenants': 'Tenants & User Accounts'
    };

    document.getElementById('page-heading').innerText = headings[tabId] || 'Dashboard';

    // Refresh tab content
    switch (tabId) {
        case 'dashboard': loadDashboard(); break;
        case 'available-rooms': loadAvailableRooms(); break;
        case 'all-rooms': loadAllRooms(); break;
        case 'payments': loadPayments(); break;
        case 'leases': loadLeases(); break;
        case 'reservations': loadReservations(); break;
        case 'maintenance': loadMaintenance(); break;
        case 'dormitories': loadDormitories(); break;
        case 'tenants': loadUsers(); break;
    }
}

// 1. DASHBOARD
async function loadDashboard() {
    try {
        const res = await fetch(`${API_BASE}/dashboard/stats`);
        const stats = await res.json();

        document.getElementById('stat-total-revenue').innerText = `₱${stats.totalRevenue.toLocaleString('en-US', { minimumFractionDigits: 2 })}`;
        document.getElementById('stat-available-rooms').innerText = stats.availableRooms;
        document.getElementById('stat-active-leases').innerText = stats.activeLeases;
        document.getElementById('stat-occupancy-rate').innerText = `${stats.overallOccupancyRate}%`;

        // Render available rooms preview
        const grid = document.getElementById('dashboard-available-rooms-grid');
        grid.innerHTML = stats.availableRoomsList.map(r => renderRoomCard(r)).join('');

        // Render recent payments
        const payTbody = document.getElementById('dashboard-recent-payments-tbody');
        payTbody.innerHTML = stats.recentPayments.length === 0
            ? '<tr><td colspan="5" style="text-align: center; color: var(--text-muted);">No payments recorded yet.</td></tr>'
            : stats.recentPayments.map(p => `
                <tr>
                    <td><strong>${escapeHtml(p.tenantName || 'Unknown')}</strong></td>
                    <td>${escapeHtml(p.roomNumber || '')}</td>
                    <td style="color: #34d399; font-weight: 600;">₱${p.amountPaid.toLocaleString()}</td>
                    <td><span class="badge badge-pending">${escapeHtml(p.paymentMethod)}</span></td>
                    <td>${new Date(p.paymentDate).toLocaleDateString()}</td>
                </tr>
            `).join('');

        // Render recent maintenance
        const maintTbody = document.getElementById('dashboard-recent-maintenance-tbody');
        maintTbody.innerHTML = stats.recentMaintenanceRequests.length === 0
            ? '<tr><td colspan="4" style="text-align: center; color: var(--text-muted);">No maintenance requests.</td></tr>'
            : stats.recentMaintenanceRequests.map(m => `
                <tr>
                    <td><strong>${escapeHtml(m.roomNumber || '')}</strong></td>
                    <td>${escapeHtml(m.issueDescription)}</td>
                    <td><span class="badge badge-${m.status === 'Resolved' ? 'available' : (m.status === 'In Progress' ? 'maintenance' : 'pending')}">${escapeHtml(m.status)}</span></td>
                    <td>${new Date(m.dateReported).toLocaleDateString()}</td>
                </tr>
            `).join('');
    } catch (err) {
        console.error('Error loading dashboard:', err);
    }
}

// 2. AVAILABLE ROOMS
async function loadAvailableRooms() {
    const dormId = document.getElementById('avail-filter-dorm').value;
    const roomType = document.getElementById('avail-filter-type').value;
    const maxRate = document.getElementById('avail-filter-rate').value;

    const params = new URLSearchParams();
    if (dormId) params.append('dormitoryId', dormId);
    if (roomType) params.append('roomType', roomType);
    if (maxRate) params.append('maxRate', maxRate);

    try {
        const res = await fetch(`${API_BASE}/rooms/available?${params.toString()}`);
        const rooms = await res.json();

        const grid = document.getElementById('available-rooms-grid');
        if (rooms.length === 0) {
            grid.innerHTML = '<div style="grid-column: 1/-1; text-align: center; padding: 48px; color: var(--text-muted);">No available rooms matching criteria.</div>';
            return;
        }

        grid.innerHTML = rooms.map(r => renderRoomCard(r)).join('');
    } catch (err) {
        console.error('Error loading available rooms:', err);
    }
}

function renderRoomCard(r) {
    const statusClass = r.status === 'Available' ? 'badge-available' : (r.status === 'Occupied' ? 'badge-occupied' : 'badge-maintenance');
    return `
        <div class="room-card">
            <div>
                <div class="room-card-header">
                    <div>
                        <div class="room-number">${escapeHtml(r.roomNumber)}</div>
                        <div class="room-dorm">🏢 ${escapeHtml(r.dormitoryName || 'Unassigned')}</div>
                    </div>
                    <span class="badge ${statusClass}">${escapeHtml(r.status)}</span>
                </div>

                <div class="room-details">
                    <div class="detail-item">
                        <div class="label">Room Type</div>
                        <div class="val">${escapeHtml(r.roomType)}</div>
                    </div>
                    <div class="detail-item">
                        <div class="label">Vacant Slots</div>
                        <div class="val" style="color: #34d399;">${r.availableSlots} of ${r.capacity} free</div>
                    </div>
                    <div class="detail-item">
                        <div class="label">Capacity</div>
                        <div class="val">${r.capacity} Person(s)</div>
                    </div>
                    <div class="detail-item">
                        <div class="label">Occupants</div>
                        <div class="val">${r.currentOccupants} Current</div>
                    </div>
                </div>

                <div class="room-rate">₱${r.monthlyRate.toLocaleString()} <span>/ month</span></div>
            </div>

            <div class="room-actions">
                ${r.isAvailable ? `
                    <button class="btn btn-primary btn-sm" onclick="quickCheckIn(${r.roomId})" style="flex: 1;">Check-In</button>
                    <button class="btn btn-outline btn-sm" onclick="quickReserve(${r.roomId})">Reserve</button>
                ` : `
                    <button class="btn btn-outline btn-sm" disabled style="flex: 1; opacity: 0.6;">Unavailable</button>
                `}
            </div>
        </div>
    `;
}

// 3. ALL ROOMS
async function loadAllRooms() {
    const search = document.getElementById('all-rooms-search').value;
    const status = document.getElementById('all-rooms-filter-status').value;

    const params = new URLSearchParams();
    if (search) params.append('search', search);
    if (status) params.append('status', status);

    try {
        const res = await fetch(`${API_BASE}/rooms?${params.toString()}`);
        const rooms = await res.json();
        const grid = document.getElementById('all-rooms-grid');

        if (rooms.length === 0) {
            grid.innerHTML = '<div style="grid-column: 1/-1; text-align: center; padding: 48px; color: var(--text-muted);">No rooms found.</div>';
            return;
        }

        grid.innerHTML = rooms.map(r => renderRoomCard(r)).join('');
    } catch (err) {
        console.error('Error loading rooms:', err);
    }
}

// 4. PAY RENT & PAYMENTS
async function loadPayments() {
    const method = document.getElementById('payment-filter-method').value;
    const status = document.getElementById('payment-filter-status').value;

    const params = new URLSearchParams();
    if (method) params.append('paymentMethod', method);
    if (status) params.append('status', status);

    try {
        const res = await fetch(`${API_BASE}/payments?${params.toString()}`);
        const payments = await res.json();
        const tbody = document.getElementById('payments-table-tbody');

        if (payments.length === 0) {
            tbody.innerHTML = '<tr><td colspan="9" style="text-align: center; padding: 32px; color: var(--text-muted);">No payment records found.</td></tr>';
            return;
        }

        tbody.innerHTML = payments.map(p => `
            <tr>
                <td><code>${escapeHtml(p.referenceNumber || 'N/A')}</code></td>
                <td><strong>${escapeHtml(p.tenantName || 'Unknown')}</strong></td>
                <td>${escapeHtml(p.roomNumber || '')}</td>
                <td>${escapeHtml(p.dormitoryName || '')}</td>
                <td style="color: #34d399; font-weight: 700;">₱${p.amountPaid.toLocaleString()}</td>
                <td><span class="badge badge-pending">${escapeHtml(p.paymentMethod)}</span></td>
                <td>${new Date(p.paymentDate).toLocaleDateString()}</td>
                <td><span class="badge badge-${p.status === 'Completed' ? 'available' : 'occupied'}">${escapeHtml(p.status)}</span></td>
                <td>
                    <button class="btn btn-outline btn-sm" onclick="viewReceipt(${p.paymentId})">🧾 View Receipt</button>
                </td>
            </tr>
        `).join('');
    } catch (err) {
        console.error('Error loading payments:', err);
    }
}

// 5. LEASES
async function loadLeases() {
    const status = document.getElementById('leases-filter-status').value;
    const params = new URLSearchParams();
    if (status) params.append('status', status);

    try {
        const res = await fetch(`${API_BASE}/leases?${params.toString()}`);
        const leases = await res.json();
        const tbody = document.getElementById('leases-table-tbody');

        if (leases.length === 0) {
            tbody.innerHTML = '<tr><td colspan="9" style="text-align: center; padding: 32px; color: var(--text-muted);">No leases found.</td></tr>';
            return;
        }

        tbody.innerHTML = leases.map(l => `
            <tr>
                <td>#${l.leaseId}</td>
                <td><strong>${escapeHtml(l.tenantName || 'Unknown')}</strong></td>
                <td>${escapeHtml(l.roomNumber || '')} (${escapeHtml(l.dormitoryName || '')})</td>
                <td>₱${l.monthlyRate.toLocaleString()}/mo</td>
                <td>${new Date(l.startDate).toLocaleDateString()} - ${new Date(l.endDate).toLocaleDateString()}</td>
                <td style="color: #34d399; font-weight: 600;">₱${l.totalAmountPaid.toLocaleString()}</td>
                <td style="color: ${l.balanceRemaining > 0 ? '#fbbf24' : '#34d399'}; font-weight: 600;">₱${l.balanceRemaining.toLocaleString()}</td>
                <td><span class="badge badge-${l.status === 'Active' ? 'available' : 'occupied'}">${escapeHtml(l.status)}</span></td>
                <td>
                    <div style="display: flex; gap: 6px;">
                        ${l.status === 'Active' ? `
                            <button class="btn btn-success btn-sm" onclick="quickPayRent(${l.leaseId}, ${l.monthlyRate})">💰 Pay Rent</button>
                            <button class="btn btn-outline btn-sm" onclick="terminateLease(${l.leaseId})" style="color: #fb7185;">Check-Out</button>
                        ` : ''}
                    </div>
                </td>
            </tr>
        `).join('');
    } catch (err) {
        console.error('Error loading leases:', err);
    }
}

// 6. RESERVATIONS
async function loadReservations() {
    const status = document.getElementById('reservations-filter-status').value;
    const params = new URLSearchParams();
    if (status) params.append('status', status);

    try {
        const res = await fetch(`${API_BASE}/reservations?${params.toString()}`);
        const reservations = await res.json();
        const tbody = document.getElementById('reservations-table-tbody');

        if (reservations.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align: center; padding: 32px; color: var(--text-muted);">No reservations found.</td></tr>';
            return;
        }

        tbody.innerHTML = reservations.map(r => `
            <tr>
                <td>#${r.reservationId}</td>
                <td><strong>${escapeHtml(r.userName || 'Unknown')}</strong><br><small style="color: var(--text-muted);">${escapeHtml(r.userPhone || r.userEmail || '')}</small></td>
                <td>${escapeHtml(r.roomNumber || '')} (${escapeHtml(r.dormitoryName || '')})</td>
                <td>₱${r.monthlyRate.toLocaleString()}/mo</td>
                <td>${new Date(r.reservationDate).toLocaleDateString()}</td>
                <td><span class="badge badge-${r.status === 'Approved' ? 'available' : (r.status === 'Pending' ? 'pending' : 'occupied')}">${escapeHtml(r.status)}</span></td>
                <td>
                    <div style="display: flex; gap: 6px;">
                        ${r.status === 'Pending' ? `
                            <button class="btn btn-success btn-sm" onclick="approveReservation(${r.reservationId})">Approve</button>
                            <button class="btn btn-outline btn-sm" onclick="cancelReservation(${r.reservationId})">Cancel</button>
                        ` : ''}
                        ${r.status === 'Approved' ? `
                            <button class="btn btn-primary btn-sm" onclick="convertReservationToLease(${r.reservationId})">Convert to Lease</button>
                        ` : ''}
                    </div>
                </td>
            </tr>
        `).join('');
    } catch (err) {
        console.error('Error loading reservations:', err);
    }
}

// 7. MAINTENANCE
async function loadMaintenance() {
    const status = document.getElementById('maintenance-filter-status').value;
    const params = new URLSearchParams();
    if (status) params.append('status', status);

    try {
        const res = await fetch(`${API_BASE}/maintenancerequests?${params.toString()}`);
        const requests = await res.json();
        const tbody = document.getElementById('maintenance-table-tbody');

        if (requests.length === 0) {
            tbody.innerHTML = '<tr><td colspan="8" style="text-align: center; padding: 32px; color: var(--text-muted);">No maintenance requests found.</td></tr>';
            return;
        }

        tbody.innerHTML = requests.map(m => `
            <tr>
                <td>#${m.requestId}</td>
                <td><strong>${escapeHtml(m.roomNumber || '')}</strong> (${escapeHtml(m.dormitoryName || '')})</td>
                <td>${escapeHtml(m.requesterName || 'Unknown')}</td>
                <td>${escapeHtml(m.issueDescription)}</td>
                <td>${new Date(m.dateReported).toLocaleDateString()}</td>
                <td><span class="badge badge-${m.status === 'Resolved' ? 'available' : (m.status === 'In Progress' ? 'maintenance' : 'pending')}">${escapeHtml(m.status)}</span></td>
                <td>${m.resolvedAt ? new Date(m.resolvedAt).toLocaleDateString() : '-'}</td>
                <td>
                    <div style="display: flex; gap: 6px;">
                        ${m.status === 'Pending' ? `
                            <button class="btn btn-outline btn-sm" onclick="updateMaintenanceStatus(${m.requestId}, 'In Progress')">Start</button>
                        ` : ''}
                        ${m.status !== 'Resolved' ? `
                            <button class="btn btn-success btn-sm" onclick="updateMaintenanceStatus(${m.requestId}, 'Resolved')">Resolve</button>
                        ` : ''}
                    </div>
                </td>
            </tr>
        `).join('');
    } catch (err) {
        console.error('Error loading maintenance requests:', err);
    }
}

// 8. DORMITORIES
async function loadDormitories() {
    try {
        const res = await fetch(`${API_BASE}/dormitories`);
        const dorms = await res.json();
        dormitoriesCache = dorms;
        const grid = document.getElementById('dormitories-grid');

        if (dorms.length === 0) {
            grid.innerHTML = '<div style="grid-column: 1/-1; text-align: center; padding: 48px; color: var(--text-muted);">No dormitories found.</div>';
            return;
        }

        grid.innerHTML = dorms.map(d => `
            <div class="room-card">
                <div>
                    <div class="room-card-header">
                        <div>
                            <div class="room-number">🏢 ${escapeHtml(d.name)}</div>
                            <div class="room-dorm">📍 ${escapeHtml(d.address || 'No address')}</div>
                        </div>
                        <span class="badge badge-available">${d.occupancyRate}% Occupied</span>
                    </div>

                    <p style="font-size: 13px; color: var(--text-secondary); margin: 8px 0;">${escapeHtml(d.description || 'No description provided.')}</p>

                    <div class="room-details">
                        <div class="detail-item">
                            <div class="label">Total Rooms</div>
                            <div class="val">${d.totalRooms}</div>
                        </div>
                        <div class="detail-item">
                            <div class="label">Vacant Rooms</div>
                            <div class="val" style="color: #34d399;">${d.availableRooms}</div>
                        </div>
                        <div class="detail-item">
                            <div class="label">Capacity</div>
                            <div class="val">${d.totalCapacity} Beds</div>
                        </div>
                        <div class="detail-item">
                            <div class="label">Occupants</div>
                            <div class="val">${d.currentOccupants} Active</div>
                        </div>
                    </div>
                </div>
            </div>
        `).join('');
    } catch (err) {
        console.error('Error loading dormitories:', err);
    }
}

// 9. TENANTS / USERS
async function loadUsers() {
    const search = document.getElementById('users-search').value;
    const role = document.getElementById('users-role-filter').value;

    const params = new URLSearchParams();
    if (search) params.append('search', search);
    if (role) params.append('role', role);

    try {
        const res = await fetch(`${API_BASE}/users?${params.toString()}`);
        const users = await res.json();
        usersCache = users;
        const tbody = document.getElementById('users-table-tbody');

        if (users.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align: center; padding: 32px; color: var(--text-muted);">No users found.</td></tr>';
            return;
        }

        tbody.innerHTML = users.map(u => `
            <tr>
                <td>#${u.userId}</td>
                <td><strong>${escapeHtml(u.fullName)}</strong></td>
                <td><code>@${escapeHtml(u.username)}</code></td>
                <td>${escapeHtml(u.email)}</td>
                <td>${escapeHtml(u.phoneNumber || '-')}</td>
                <td><span class="badge badge-${u.role === 'Tenant' ? 'pending' : 'available'}">${escapeHtml(u.role)}</span></td>
                <td>${u.activeLeasesCount} Active</td>
            </tr>
        `).join('');
    } catch (err) {
        console.error('Error loading users:', err);
    }
}

// MODAL HANDLERS & ACTIONS
function openPayRentModal() {
    document.getElementById('modal-pay-rent').classList.add('active');
}

function quickPayRent(leaseId, monthlyRate) {
    openPayRentModal();
    const select = document.getElementById('pay-lease-id');
    select.value = leaseId;
    document.getElementById('pay-amount').value = monthlyRate;
}

function updatePayRentAutoAmount() {
    const select = document.getElementById('pay-lease-id');
    const selectedOption = select.options[select.selectedIndex];
    if (selectedOption && selectedOption.dataset.rate) {
        document.getElementById('pay-amount').value = selectedOption.dataset.rate;
    }
}

async function handlePayRent(e) {
    e.preventDefault();
    const leaseId = parseInt(document.getElementById('pay-lease-id').value);
    const amountPaid = parseFloat(document.getElementById('pay-amount').value);
    const paymentMethod = document.getElementById('pay-method').value;
    const referenceNumber = document.getElementById('pay-reference').value;

    try {
        const res = await fetch(`${API_BASE}/payments`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ leaseId, amountPaid, paymentMethod, referenceNumber })
        });

        if (!res.ok) {
            const err = await res.json();
            showToast(err.message || 'Payment failed.', 'error');
            return;
        }

        const payment = await res.json();
        closeModal('modal-pay-rent');
        showToast(`Rent payment of ₱${payment.amountPaid.toLocaleString()} recorded successfully!`, 'success');

        // View receipt
        viewReceipt(payment.paymentId);

        // Refresh views
        await loadInitialData();
        loadDashboard();
        loadPayments();
        loadLeases();
    } catch (err) {
        showToast('Error processing payment.', 'error');
    }
}

async function viewReceipt(paymentId) {
    try {
        const res = await fetch(`${API_BASE}/payments/${paymentId}`);
        const p = await res.json();

        const box = document.getElementById('receipt-details');
        box.innerHTML = `
            <div style="text-align: center; margin-bottom: 16px;">
                <h4 style="font-size: 16px; font-weight: 700; color: #60a5fa;">OFFICIAL RENT RECEIPT</h4>
                <div style="font-size: 12px; color: var(--text-muted);">Ref: ${escapeHtml(p.referenceNumber || 'N/A')}</div>
            </div>
            <div class="receipt-row">
                <span style="color: var(--text-muted);">Date & Time:</span>
                <span>${new Date(p.paymentDate).toLocaleString()}</span>
            </div>
            <div class="receipt-row">
                <span style="color: var(--text-muted);">Tenant Name:</span>
                <strong>${escapeHtml(p.tenantName || 'Unknown')}</strong>
            </div>
            <div class="receipt-row">
                <span style="color: var(--text-muted);">Room & Dormitory:</span>
                <span>${escapeHtml(p.roomNumber || '')} - ${escapeHtml(p.dormitoryName || '')}</span>
            </div>
            <div class="receipt-row">
                <span style="color: var(--text-muted);">Payment Method:</span>
                <span>${escapeHtml(p.paymentMethod)}</span>
            </div>
            <div class="receipt-row">
                <span style="color: var(--text-muted);">Payment Status:</span>
                <span style="color: #34d399; font-weight: 600;">${escapeHtml(p.status)}</span>
            </div>
            <div class="receipt-row total">
                <span>AMOUNT PAID:</span>
                <span>₱${p.amountPaid.toLocaleString('en-US', { minimumFractionDigits: 2 })}</span>
            </div>
        `;

        document.getElementById('modal-receipt').classList.add('active');
    } catch (err) {
        showToast('Could not load receipt.', 'error');
    }
}

function openNewLeaseModal() {
    // Set default dates
    const today = new Date().toISOString().split('T')[0];
    const next6Months = new Date();
    next6Months.setMonth(next6Months.getMonth() + 6);

    document.getElementById('lease-start-date').value = today;
    document.getElementById('lease-end-date').value = next6Months.toISOString().split('T')[0];
    document.getElementById('modal-new-lease').classList.add('active');
}

function quickCheckIn(roomId) {
    openNewLeaseModal();
    document.getElementById('lease-room-id').value = roomId;
}

async function handleNewLease(e) {
    e.preventDefault();
    const userId = parseInt(document.getElementById('lease-user-id').value);
    const roomId = parseInt(document.getElementById('lease-room-id').value);
    const startDate = document.getElementById('lease-start-date').value;
    const endDate = document.getElementById('lease-end-date').value;

    try {
        const res = await fetch(`${API_BASE}/leases`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId, roomId, startDate, endDate, status: 'Active' })
        });

        if (!res.ok) {
            const err = await res.json();
            showToast(err.message || 'Check-in failed.', 'error');
            return;
        }

        closeModal('modal-new-lease');
        showToast('Tenant checked in successfully!', 'success');
        await loadInitialData();
        loadDashboard();
        loadLeases();
        loadAvailableRooms();
    } catch (err) {
        showToast('Error creating lease.', 'error');
    }
}

async function terminateLease(leaseId) {
    if (!confirm('Are you sure you want to terminate this lease and check-out the tenant?')) return;

    try {
        const res = await fetch(`${API_BASE}/leases/${leaseId}/terminate`, { method: 'POST' });
        if (!res.ok) {
            const err = await res.json();
            showToast(err.message || 'Termination failed.', 'error');
            return;
        }

        showToast('Tenant checked out and room occupancy updated.', 'success');
        await loadInitialData();
        loadDashboard();
        loadLeases();
        loadAvailableRooms();
    } catch (err) {
        showToast('Error terminating lease.', 'error');
    }
}

function openReserveModal() {
    document.getElementById('modal-reserve').classList.add('active');
}

function quickReserve(roomId) {
    openReserveModal();
    document.getElementById('reserve-room-id').value = roomId;
}

async function handleNewReservation(e) {
    e.preventDefault();
    const userId = parseInt(document.getElementById('reserve-user-id').value);
    const roomId = parseInt(document.getElementById('reserve-room-id').value);

    try {
        const res = await fetch(`${API_BASE}/reservations`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId, roomId, status: 'Pending' })
        });

        if (!res.ok) {
            const err = await res.json();
            showToast(err.message || 'Reservation failed.', 'error');
            return;
        }

        closeModal('modal-reserve');
        showToast('Room reserved successfully! Status: Pending Approval', 'success');
        loadReservations();
        loadDashboard();
    } catch (err) {
        showToast('Error submitting reservation.', 'error');
    }
}

async function approveReservation(id) {
    try {
        const res = await fetch(`${API_BASE}/reservations/${id}/approve`, { method: 'PUT' });
        if (res.ok) {
            showToast('Reservation approved.', 'success');
            loadReservations();
        }
    } catch (err) {
        showToast('Error approving reservation.', 'error');
    }
}

async function cancelReservation(id) {
    try {
        const res = await fetch(`${API_BASE}/reservations/${id}/cancel`, { method: 'PUT' });
        if (res.ok) {
            showToast('Reservation cancelled.', 'success');
            loadReservations();
        }
    } catch (err) {
        showToast('Error cancelling reservation.', 'error');
    }
}

async function convertReservationToLease(id) {
    const today = new Date().toISOString().split('T')[0];
    const next6Months = new Date();
    next6Months.setMonth(next6Months.getMonth() + 6);

    try {
        const res = await fetch(`${API_BASE}/reservations/${id}/convert-to-lease`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ startDate: today, endDate: next6Months.toISOString().split('T')[0] })
        });

        if (!res.ok) {
            const err = await res.json();
            showToast(err.message || 'Conversion failed.', 'error');
            return;
        }

        showToast('Reservation converted to active lease!', 'success');
        await loadInitialData();
        loadReservations();
        loadLeases();
        loadDashboard();
    } catch (err) {
        showToast('Error converting reservation.', 'error');
    }
}

function openMaintenanceModal() {
    document.getElementById('modal-maintenance').classList.add('active');
}

async function handleNewMaintenance(e) {
    e.preventDefault();
    const roomId = parseInt(document.getElementById('maint-room-id').value);
    const userId = parseInt(document.getElementById('maint-user-id').value);
    const issueDescription = document.getElementById('maint-description').value;

    try {
        const res = await fetch(`${API_BASE}/maintenancerequests`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ roomId, userId, issueDescription, status: 'Pending' })
        });

        if (!res.ok) {
            const err = await res.json();
            showToast(err.message || 'Failed to submit request.', 'error');
            return;
        }

        closeModal('modal-maintenance');
        showToast('Maintenance request submitted successfully!', 'success');
        document.getElementById('form-maintenance').reset();
        loadMaintenance();
        loadDashboard();
    } catch (err) {
        showToast('Error creating maintenance request.', 'error');
    }
}

async function updateMaintenanceStatus(id, newStatus) {
    try {
        const res = await fetch(`${API_BASE}/maintenancerequests/${id}/status`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status: newStatus })
        });

        if (res.ok) {
            showToast(`Maintenance status updated to ${newStatus}.`, 'success');
            loadMaintenance();
            loadDashboard();
        }
    } catch (err) {
        showToast('Error updating maintenance status.', 'error');
    }
}

function openNewRoomModal() {
    document.getElementById('modal-room').classList.add('active');
}

async function handleNewRoom(e) {
    e.preventDefault();
    const dormitoryId = parseInt(document.getElementById('room-dorm-id').value);
    const roomNumber = document.getElementById('room-number').value;
    const roomType = document.getElementById('room-type').value;
    const capacity = parseInt(document.getElementById('room-capacity').value);
    const monthlyRate = parseFloat(document.getElementById('room-rate').value);

    try {
        const res = await fetch(`${API_BASE}/rooms`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ dormitoryId, roomNumber, roomType, capacity, monthlyRate, status: 'Available' })
        });

        if (!res.ok) {
            const err = await res.json();
            showToast(err.message || 'Failed to add room.', 'error');
            return;
        }

        closeModal('modal-room');
        showToast(`Room '${roomNumber}' added successfully!`, 'success');
        document.getElementById('form-room').reset();
        await loadInitialData();
        loadAllRooms();
        loadAvailableRooms();
        loadDashboard();
    } catch (err) {
        showToast('Error creating room.', 'error');
    }
}

async function seedSampleData() {
    if (!confirm('Reseed database with fresh sample data for all models?')) return;

    try {
        const res = await fetch(`${API_BASE}/seed/reset`, { method: 'POST' });
        if (res.ok) {
            showToast('Database reset and reseeded with demo data!', 'success');
            await loadInitialData();
            loadDashboard();
        }
    } catch (err) {
        showToast('Error reseeding data.', 'error');
    }
}

// Helpers
function closeModal(modalId) {
    document.getElementById(modalId).classList.remove('active');
}

function showToast(message, type = 'info') {
    const container = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerText = message;
    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => toast.remove(), 300);
    }, 3500);
}

function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/&/g, '&amp;')
              .replace(/</g, '&lt;')
              .replace(/>/g, '&gt;')
              .replace(/"/g, '&quot;')
              .replace(/'/g, '&#039;');
}
