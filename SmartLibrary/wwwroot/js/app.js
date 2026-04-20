// Smart Library Management System - Frontend JavaScript

// API Base URL
const API_BASE = '/api';

// State
let currentUser = null;
let authToken = null;

// DOM Elements
const loginPage = document.getElementById('login-page');
const dashboardPage = document.getElementById('dashboard-page');
const loginForm = document.getElementById('login-form');
const loginError = document.getElementById('login-error');
const userInfo = document.getElementById('user-info');
const logoutBtn = document.getElementById('logout-btn');
const modalOverlay = document.getElementById('modal-overlay');
const modalTitle = document.getElementById('modal-title');
const modalContent = document.getElementById('modal-content');
const toast = document.getElementById('toast');

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    checkAuth();
    setupEventListeners();
});

// Authentication
function checkAuth() {
    const token = localStorage.getItem('authToken');
    const user = localStorage.getItem('currentUser');
    
    if (token && user) {
        authToken = token;
        currentUser = JSON.parse(user);
        showDashboard();
    } else {
        showLoginPage();
    }
}

async function login(username, password) {
    try {
        const response = await fetch(`${API_BASE}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, password })
        });
        
        const result = await response.json();
        
        if (result.code === 200) {
            authToken = result.data.token;
            currentUser = {
                username: result.data.username,
                role: result.data.role,
                userId: result.data.userId
            };
            
            localStorage.setItem('authToken', authToken);
            localStorage.setItem('currentUser', JSON.stringify(currentUser));
            
            showToast('登录成功', 'success');
            showDashboard();
            return true;
        } else {
            throw new Error(result.message || '登录失败');
        }
    } catch (error) {
        showToast(error.message, 'error');
        return false;
    }
}

function logout() {
    authToken = null;
    currentUser = null;
    localStorage.removeItem('authToken');
    localStorage.removeItem('currentUser');
    showLoginPage();
}

function showLoginPage() {
    loginPage.classList.remove('hidden');
    dashboardPage.classList.add('hidden');
}

function showDashboard() {
    loginPage.classList.add('hidden');
    dashboardPage.classList.remove('hidden');
    userInfo.textContent = `${currentUser.username} (${getRoleName(currentUser.role)})`;
    loadBooks();
}

function getRoleName(role) {
    const roles = { 'Admin': '管理员', 'Teacher': '教师', 'Student': '学生', 'User': '用户' };
    return roles[role] || role;
}

// API Helper
async function apiRequest(url, options = {}) {
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };
    
    if (authToken) {
        headers['Authorization'] = `Bearer ${authToken}`;
    }
    
    const response = await fetch(`${API_BASE}${url}`, {
        ...options,
        headers
    });
    
    const result = await response.json();
    
    if (result.code !== 200 && result.code !== 201) {
        throw new Error(result.message || '请求失败');
    }
    
    return result;
}

// Event Listeners
function setupEventListeners() {
    // Login
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;
        loginError.textContent = '';
        
        const success = await login(username, password);
        if (!success) {
            loginError.textContent = '用户名或密码错误';
        }
    });

    // Logout
    logoutBtn.addEventListener('click', logout);

    // Navigation
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const page = link.dataset.page;
            switchPage(page);
        });
    });

    // Modal close
    document.querySelector('.modal-close').addEventListener('click', closeModal);
    document.querySelector('.modal-cancel').addEventListener('click', closeModal);
    modalOverlay.addEventListener('click', (e) => {
        if (e.target === modalOverlay) closeModal();
    });

    // Add buttons
    document.getElementById('add-book-btn').addEventListener('click', () => showBookModal());
    document.getElementById('add-borrow-btn').addEventListener('click', () => showBorrowModal());
    document.getElementById('add-device-btn').addEventListener('click', () => showDeviceModal());
    document.getElementById('add-reservation-btn').addEventListener('click', () => showReservationModal());
    document.getElementById('add-user-btn').addEventListener('click', () => showUserModal());

    // Search and filters
    document.getElementById('book-search').addEventListener('input', debounce(loadBooks, 300));
    document.getElementById('book-category-filter').addEventListener('change', loadBooks);
    document.getElementById('borrow-status-filter').addEventListener('change', loadBorrowRecords);
}

// Page Switching
function switchPage(page) {
    document.querySelectorAll('.section').forEach(s => s.classList.add('hidden'));
    document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));
    
    document.getElementById(`${page}-section`).classList.remove('hidden');
    document.querySelector(`[data-page="${page}"]`).classList.add('active');
    
    // Load data based on page
    switch(page) {
        case 'books': loadBooks(); break;
        case 'borrow': loadBorrowRecords(); break;
        case 'devices': loadDevices(); break;
        case 'reservations': loadReservations(); break;
        case 'users': loadUsers(); break;
    }
}

// Books
async function loadBooks() {
    const loading = document.getElementById('books-loading');
    const tbody = document.getElementById('books-tbody');
    const search = document.getElementById('book-search').value;
    const category = document.getElementById('book-category-filter').value;
    
    loading.classList.remove('hidden');
    tbody.innerHTML = '';
    
    try {
        let url = '/books?';
        if (search) url += `search=${encodeURIComponent(search)}&`;
        if (category) url += `category=${encodeURIComponent(category)}&`;
        
        const result = await apiRequest(url);
        const books = result.data || [];
        
        books.forEach(book => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${book.id}</td>
                <td>${escapeHtml(book.title)}</td>
                <td>${escapeHtml(book.author)}</td>
                <td>${escapeHtml(book.isbn)}</td>
                <td>${escapeHtml(book.category)}</td>
                <td>${book.totalCopies}</td>
                <td>${book.availableCopies}</td>
                <td>${escapeHtml(book.location)}</td>
                <td>¥${book.price.toFixed(2)}</td>
                <td class="actions">
                    <button class="btn btn-small btn-primary" onclick="showBookModal(${book.id})">编辑</button>
                    <button class="btn btn-small btn-danger" onclick="deleteBook(${book.id})">删除</button>
                </td>
            `;
            tbody.appendChild(row);
        });
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        loading.classList.add('hidden');
    }
}

async function showBookModal(id = null) {
    let book = null;
    if (id) {
        try {
            const result = await apiRequest(`/books/${id}`);
            book = result.data;
        } catch (error) {
            showToast(error.message, 'error');
            return;
        }
    }
    
    modalTitle.textContent = id ? '编辑图书' : '添加图书';
    modalContent.innerHTML = `
        <form id="book-form">
            <div class="form-group">
                <label>书名</label>
                <input type="text" name="title" value="${book?.title || ''}" required>
            </div>
            <div class="form-group">
                <label>作者</label>
                <input type="text" name="author" value="${book?.author || ''}" required>
            </div>
            <div class="form-group">
                <label>ISBN</label>
                <input type="text" name="isbn" value="${book?.isbn || ''}" required>
            </div>
            <div class="form-group">
                <label>分类</label>
                <select name="category">
                    <option value="Technology" ${book?.category === 'Technology' ? 'selected' : ''}>技术</option>
                    <option value="Computer Science" ${book?.category === 'Computer Science' ? 'selected' : ''}>计算机科学</option>
                    <option value="AI" ${book?.category === 'AI' ? 'selected' : ''}>人工智能</option>
                    <option value="Programming" ${book?.category === 'Programming' ? 'selected' : ''}>编程</option>
                </select>
            </div>
            <div class="form-group">
                <label>总数量</label>
                <input type="number" name="totalCopies" value="${book?.totalCopies || 1}" min="1" required>
            </div>
            <div class="form-group">
                <label>可借数量</label>
                <input type="number" name="availableCopies" value="${book?.availableCopies ?? book?.totalCopies || 1}" min="0" required>
            </div>
            <div class="form-group">
                <label>位置</label>
                <input type="text" name="location" value="${book?.location || ''}" required>
            </div>
            <div class="form-group">
                <label>价格</label>
                <input type="number" name="price" value="${book?.price || 0}" step="0.01" min="0" required>
            </div>
            <div class="form-group">
                <label>出版日期</label>
                <input type="date" name="publishedDate" value="${book?.publishedDate ? new Date(book.publishedDate).toISOString().split('T')[0] : ''}">
            </div>
        </form>
    `;
    
    document.querySelector('.modal-confirm').onclick = async () => {
        const form = document.getElementById('book-form');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        const formData = new FormData(form);
        const data = {
            title: formData.get('title'),
            author: formData.get('author'),
            isbn: formData.get('isbn'),
            category: formData.get('category'),
            totalCopies: parseInt(formData.get('totalCopies')),
            availableCopies: parseInt(formData.get('availableCopies')),
            location: formData.get('location'),
            price: parseFloat(formData.get('price')),
            publishedDate: formData.get('publishedDate') ? new Date(formData.get('publishedDate')) : new Date()
        };
        
        try {
            if (id) {
                await apiRequest(`/books/${id}`, { method: 'PUT', body: JSON.stringify(data) });
                showToast('更新成功', 'success');
            } else {
                await apiRequest('/books', { method: 'POST', body: JSON.stringify(data) });
                showToast('添加成功', 'success');
            }
            closeModal();
            loadBooks();
        } catch (error) {
            showToast(error.message, 'error');
        }
    };
    
    openModal();
}

async function deleteBook(id) {
    if (!confirm('确定要删除这本书吗？')) return;
    
    try {
        await apiRequest(`/books/${id}`, { method: 'DELETE' });
        showToast('删除成功', 'success');
        loadBooks();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

// Borrow Records
async function loadBorrowRecords() {
    const loading = document.getElementById('borrow-loading');
    const tbody = document.getElementById('borrow-tbody');
    const status = document.getElementById('borrow-status-filter').value;
    
    loading.classList.remove('hidden');
    tbody.innerHTML = '';
    
    try {
        let url = '/borrowrecords?';
        if (status) url += `status=${encodeURIComponent(status)}&`;
        
        const result = await apiRequest(url);
        const records = result.data || [];
        
        records.forEach(record => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${record.id}</td>
                <td>${escapeHtml(record.userName || '-')}</td>
                <td>${escapeHtml(record.bookTitle || '-')}</td>
                <td>${formatDate(record.borrowDate)}</td>
                <td>${formatDate(record.dueDate)}</td>
                <td>${record.returnDate ? formatDate(record.returnDate) : '-'}</td>
                <td><span class="status-badge ${record.status.toLowerCase()}">${getStatusText(record.status)}</span></td>
                <td class="actions">
                    ${record.status === 'Borrowed' ? `<button class="btn btn-small btn-success" onclick="returnBook(${record.id})">归还</button>` : ''}
                    <button class="btn btn-small btn-danger" onclick="deleteBorrowRecord(${record.id})">删除</button>
                </td>
            `;
            tbody.appendChild(row);
        });
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        loading.classList.add('hidden');
    }
}

async function showBorrowModal() {
    let users, books;
    try {
        const [usersRes, booksRes] = await Promise.all([
            apiRequest('/auth/users'),
            apiRequest('/books')
        ]);
        users = usersRes.data || [];
        books = booksRes.data || [];
    } catch (error) {
        showToast(error.message, 'error');
        return;
    }
    
    modalTitle.textContent = '新增借阅';
    modalContent.innerHTML = `
        <form id="borrow-form">
            <div class="form-group">
                <label>用户</label>
                <select name="userId" required>
                    <option value="">选择用户</option>
                    ${users.map(u => `<option value="${u.id}">${escapeHtml(u.username)} (${getRoleName(u.role)})</option>`).join('')}
                </select>
            </div>
            <div class="form-group">
                <label>图书</label>
                <select name="bookId" required>
                    <option value="">选择图书</option>
                    ${books.filter(b => b.availableCopies > 0).map(b => `<option value="${b.id}">${escapeHtml(b.title)} (可借: ${b.availableCopies})</option>`).join('')}
                </select>
            </div>
            <div class="form-group">
                <label>借阅天数</label>
                <input type="number" name="days" value="30" min="1" max="90" required>
            </div>
        </form>
    `;
    
    document.querySelector('.modal-confirm').onclick = async () => {
        const form = document.getElementById('borrow-form');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        const formData = new FormData(form);
        const data = {
            userId: parseInt(formData.get('userId')),
            bookId: parseInt(formData.get('bookId')),
            days: parseInt(formData.get('days'))
        };
        
        try {
            await apiRequest('/borrowrecords/borrow', { method: 'POST', body: JSON.stringify(data) });
            showToast('借阅成功', 'success');
            closeModal();
            loadBorrowRecords();
        } catch (error) {
            showToast(error.message, 'error');
        }
    };
    
    openModal();
}

async function returnBook(id) {
    if (!confirm('确认归还这本书？')) return;
    
    try {
        await apiRequest(`/borrowrecords/${id}/return`, { method: 'POST' });
        showToast('归还成功', 'success');
        loadBorrowRecords();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

async function deleteBorrowRecord(id) {
    if (!confirm('确定要删除这条借阅记录吗？')) return;
    
    try {
        await apiRequest(`/borrowrecords/${id}`, { method: 'DELETE' });
        showToast('删除成功', 'success');
        loadBorrowRecords();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

// Devices
async function loadDevices() {
    const loading = document.getElementById('devices-loading');
    const tbody = document.getElementById('devices-tbody');
    
    loading.classList.remove('hidden');
    tbody.innerHTML = '';
    
    try {
        const result = await apiRequest('/devices');
        const devices = result.data || [];
        
        devices.forEach(device => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${device.id}</td>
                <td>${escapeHtml(device.deviceName)}</td>
                <td>${escapeHtml(device.deviceType)}</td>
                <td>${escapeHtml(device.location)}</td>
                <td><span class="status-badge ${device.status.toLowerCase()}">${getStatusText(device.status)}</span></td>
                <td>${formatDate(device.lastMaintenance)}</td>
                <td class="actions">
                    <button class="btn btn-small btn-primary" onclick="showDeviceModal(${device.id})">编辑</button>
                    <button class="btn btn-small btn-danger" onclick="deleteDevice(${device.id})">删除</button>
                </td>
            `;
            tbody.appendChild(row);
        });
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        loading.classList.add('hidden');
    }
}

async function showDeviceModal(id = null) {
    let device = null;
    if (id) {
        try {
            const result = await apiRequest(`/devices/${id}`);
            device = result.data;
        } catch (error) {
            showToast(error.message, 'error');
            return;
        }
    }
    
    modalTitle.textContent = id ? '编辑设备' : '添加设备';
    modalContent.innerHTML = `
        <form id="device-form">
            <div class="form-group">
                <label>设备名称</label>
                <input type="text" name="deviceName" value="${device?.deviceName || ''}" required>
            </div>
            <div class="form-group">
                <label>设备类型</label>
                <select name="deviceType">
                    <option value="RFID" ${device?.deviceType === 'RFID' ? 'selected' : ''}>RFID</option>
                    <option value="SelfCheckout" ${device?.deviceType === 'SelfCheckout' ? 'selected' : ''}>自助借还</option>
                    <option value="Kiosk" ${device?.deviceType === 'Kiosk' ? 'selected' : ''}>查询终端</option>
                </select>
            </div>
            <div class="form-group">
                <label>位置</label>
                <input type="text" name="location" value="${device?.location || ''}" required>
            </div>
            <div class="form-group">
                <label>状态</label>
                <select name="status">
                    <option value="Online" ${device?.status === 'Online' ? 'selected' : ''}>在线</option>
                    <option value="Offline" ${device?.status === 'Offline' ? 'selected' : ''}>离线</option>
                    <option value="Maintenance" ${device?.status === 'Maintenance' ? 'selected' : ''}>维护中</option>
                </select>
            </div>
            <div class="form-group">
                <label>最后维护日期</label>
                <input type="date" name="lastMaintenance" value="${device?.lastMaintenance ? new Date(device.lastMaintenance).toISOString().split('T')[0] : new Date().toISOString().split('T')[0]}">
            </div>
        </form>
    `;
    
    document.querySelector('.modal-confirm').onclick = async () => {
        const form = document.getElementById('device-form');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        const formData = new FormData(form);
        const data = {
            deviceName: formData.get('deviceName'),
            deviceType: formData.get('deviceType'),
            location: formData.get('location'),
            status: formData.get('status'),
            lastMaintenance: new Date(formData.get('lastMaintenance'))
        };
        
        try {
            if (id) {
                await apiRequest(`/devices/${id}`, { method: 'PUT', body: JSON.stringify(data) });
                showToast('更新成功', 'success');
            } else {
                await apiRequest('/devices', { method: 'POST', body: JSON.stringify(data) });
                showToast('添加成功', 'success');
            }
            closeModal();
            loadDevices();
        } catch (error) {
            showToast(error.message, 'error');
        }
    };
    
    openModal();
}

async function deleteDevice(id) {
    if (!confirm('确定要删除这个设备吗？')) return;
    
    try {
        await apiRequest(`/devices/${id}`, { method: 'DELETE' });
        showToast('删除成功', 'success');
        loadDevices();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

// Reservations
async function loadReservations() {
    const loading = document.getElementById('reservations-loading');
    const tbody = document.getElementById('reservations-tbody');
    
    loading.classList.remove('hidden');
    tbody.innerHTML = '';
    
    try {
        const result = await apiRequest('/reservations');
        const reservations = result.data || [];
        
        reservations.forEach(reservation => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${reservation.id}</td>
                <td>${escapeHtml(reservation.userName || '-')}</td>
                <td>${escapeHtml(reservation.bookTitle || '-')}</td>
                <td>${formatDate(reservation.reservationDate)}</td>
                <td>${formatDate(reservation.expiryDate)}</td>
                <td><span class="status-badge ${reservation.status.toLowerCase()}">${getStatusText(reservation.status)}</span></td>
                <td class="actions">
                    ${reservation.status === 'Pending' ? `<button class="btn btn-small btn-success" onclick="updateReservationStatus(${reservation.id}, 'Fulfilled')">完成</button>` : ''}
                    ${reservation.status === 'Pending' ? `<button class="btn btn-small btn-danger" onclick="updateReservationStatus(${reservation.id}, 'Cancelled')">取消</button>` : ''}
                    <button class="btn btn-small btn-danger" onclick="deleteReservation(${reservation.id})">删除</button>
                </td>
            `;
            tbody.appendChild(row);
        });
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        loading.classList.add('hidden');
    }
}

async function showReservationModal() {
    let users, books;
    try {
        const [usersRes, booksRes] = await Promise.all([
            apiRequest('/auth/users'),
            apiRequest('/books')
        ]);
        users = usersRes.data || [];
        books = booksRes.data || [];
    } catch (error) {
        showToast(error.message, 'error');
        return;
    }
    
    modalTitle.textContent = '新增预约';
    modalContent.innerHTML = `
        <form id="reservation-form">
            <div class="form-group">
                <label>用户</label>
                <select name="userId" required>
                    <option value="">选择用户</option>
                    ${users.map(u => `<option value="${u.id}">${escapeHtml(u.username)} (${getRoleName(u.role)})</option>`).join('')}
                </select>
            </div>
            <div class="form-group">
                <label>图书</label>
                <select name="bookId" required>
                    <option value="">选择图书</option>
                    ${books.map(b => `<option value="${b.id}">${escapeHtml(b.title)}</option>`).join('')}
                </select>
            </div>
        </form>
    `;
    
    document.querySelector('.modal-confirm').onclick = async () => {
        const form = document.getElementById('reservation-form');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        const formData = new FormData(form);
        const data = {
            userId: parseInt(formData.get('userId')),
            bookId: parseInt(formData.get('bookId'))
        };
        
        try {
            await apiRequest('/reservations', { method: 'POST', body: JSON.stringify(data) });
            showToast('预约成功', 'success');
            closeModal();
            loadReservations();
        } catch (error) {
            showToast(error.message, 'error');
        }
    };
    
    openModal();
}

async function updateReservationStatus(id, status) {
    try {
        await apiRequest(`/reservations/${id}/status`, { 
            method: 'PUT', 
            body: JSON.stringify({ status }) 
        });
        showToast('更新成功', 'success');
        loadReservations();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

async function deleteReservation(id) {
    if (!confirm('确定要删除这条预约记录吗？')) return;
    
    try {
        await apiRequest(`/reservations/${id}`, { method: 'DELETE' });
        showToast('删除成功', 'success');
        loadReservations();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

// Users
async function loadUsers() {
    const loading = document.getElementById('users-loading');
    const tbody = document.getElementById('users-tbody');
    
    loading.classList.remove('hidden');
    tbody.innerHTML = '';
    
    try {
        const result = await apiRequest('/auth/users');
        const users = result.data || [];
        
        users.forEach(user => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${user.id}</td>
                <td>${escapeHtml(user.username)}</td>
                <td>${escapeHtml(user.realName || '-')}</td>
                <td>${escapeHtml(user.email)}</td>
                <td>${escapeHtml(user.department || '-')}</td>
                <td>${escapeHtml(user.position || '-')}</td>
                <td>${getRoleName(user.role)}</td>
                <td>${formatDate(user.createdAt)}</td>
                <td class="actions">
                    <button class="btn btn-small btn-primary" onclick="showUserModal(${user.id})">编辑</button>
                    <button class="btn btn-small btn-danger" onclick="deleteUser(${user.id})">删除</button>
                </td>
            `;
            tbody.appendChild(row);
        });
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        loading.classList.add('hidden');
    }
}

async function showUserModal(id = null) {
    let user = null;
    if (id) {
        try {
            const result = await apiRequest(`/auth/users/${id}`);
            user = result.data;
        } catch (error) {
            showToast(error.message, 'error');
            return;
        }
    }
    
    modalTitle.textContent = id ? '编辑用户' : '添加用户';
    modalContent.innerHTML = `
        <form id="user-form">
            ${!id ? `
            <div class="form-group">
                <label>用户名</label>
                <input type="text" name="username" required>
            </div>
            <div class="form-group">
                <label>密码</label>
                <input type="password" name="password" required>
            </div>
            ` : `
            <div class="form-group">
                <label>新密码 (留空则不修改)</label>
                <input type="password" name="password">
            </div>
            `}
            <div class="form-group">
                <label>邮箱</label>
                <input type="email" name="email" value="${user?.email || ''}" required>
            </div>
            <div class="form-group">
                <label>姓名</label>
                <input type="text" name="realName" value="${user?.realName || ''}">
            </div>
            <div class="form-group">
                <label>院系</label>
                <input type="text" name="department" value="${user?.department || ''}">
            </div>
            <div class="form-group">
                <label>职务</label>
                <input type="text" name="position" value="${user?.position || ''}">
            </div>
            <div class="form-group">
                <label>角色</label>
                <select name="role">
                    <option value="User" ${user?.role === 'User' ? 'selected' : ''}>普通用户</option>
                    <option value="Student" ${user?.role === 'Student' ? 'selected' : ''}>学生</option>
                    <option value="Teacher" ${user?.role === 'Teacher' ? 'selected' : ''}>教师</option>
                    <option value="Admin" ${user?.role === 'Admin' ? 'selected' : ''}>管理员</option>
                </select>
            </div>
        </form>
    `;
    
    document.querySelector('.modal-confirm').onclick = async () => {
        const form = document.getElementById('user-form');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        const formData = new FormData(form);
        const data = {
            email: formData.get('email'),
            role: formData.get('role'),
            realName: formData.get('realName') || null,
            department: formData.get('department') || null,
            position: formData.get('position') || null,
            password: formData.get('password') || undefined
        };
        
        try {
            if (id) {
                await apiRequest(`/auth/users/${id}`, { method: 'PUT', body: JSON.stringify(data) });
                showToast('更新成功', 'success');
            } else {
                data.username = formData.get('username');
                await apiRequest('/auth/register', { method: 'POST', body: JSON.stringify(data) });
                showToast('添加成功', 'success');
            }
            closeModal();
            loadUsers();
        } catch (error) {
            showToast(error.message, 'error');
        }
    };
    
    openModal();
}

async function deleteUser(id) {
    if (!confirm('确定要删除这个用户吗？')) return;
    
    try {
        await apiRequest(`/auth/users/${id}`, { method: 'DELETE' });
        showToast('删除成功', 'success');
        loadUsers();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

// Utility Functions
function openModal() {
    modalOverlay.classList.remove('hidden');
}

function closeModal() {
    modalOverlay.classList.add('hidden');
}

function showToast(message, type = 'success') {
    toast.textContent = message;
    toast.className = `toast ${type}`;
    toast.classList.remove('hidden');
    
    setTimeout(() => {
        toast.classList.add('hidden');
    }, 3000);
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function formatDate(dateString) {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleDateString('zh-CN');
}

function getStatusText(status) {
    const statusMap = {
        'Online': '在线',
        'Offline': '离线',
        'Maintenance': '维护中',
        'Borrowed': '借阅中',
        'Returned': '已归还',
        'Overdue': '逾期',
        'Pending': '待处理',
        'Fulfilled': '已完成',
        'Cancelled': '已取消',
        'Expired': '已过期'
    };
    return statusMap[status] || status;
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}
