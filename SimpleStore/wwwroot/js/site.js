// Toast notification functionality
function showToast(message, type = 'success') {
    // Remove existing toast if any
    const existingToast = document.getElementById('toast');
    if (existingToast) {
        existingToast.remove();
    }

    // Create toast element
    const toast = document.createElement('div');
    toast.id = 'toast';
    toast.className = 'toast';

    const icon = type === 'success' ? '✓' : type === 'error' ? '✗' : 'ℹ';
    const borderColor = type === 'success' ? 'var(--success-color)' :
        type === 'error' ? 'var(--danger-color)' :
            'var(--primary-color)';

    toast.style.borderRightColor = borderColor;
    toast.innerHTML = `
        <span class="toast-icon" style="color: ${borderColor}">${icon}</span>
        <span>${message}</span>
    `;

    document.body.appendChild(toast);

    // Show toast
    setTimeout(() => {
        toast.classList.add('show');
    }, 100);

    // Hide toast after 3 seconds
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => {
            if (toast && toast.parentNode) {
                toast.remove();
            }
        }, 300);
    }, 3000);
}

// Add to cart functionality (for AJAX requests)
function addToCart(productId, productName) {
    // Show loading state
    const button = event.target;
    const originalText = button.textContent;
    button.textContent = 'در حال افزودن...';
    button.disabled = true;

    // Make AJAX request to add item to cart
    fetch('/Cart/Add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ productId: productId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showToast(`${productName} به سبد خرید اضافه شد!`);
                updateCartBadge(data.cartCount);
            } else {
                showToast(data.message || 'خطا در افزودن به سبد خرید', 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('خطا در افزودن به سبد خرید', 'error');
        })
        .finally(() => {
            button.textContent = originalText;
            button.disabled = false;
        });
}

// Update cart badge count
function updateCartBadge(count) {
    const badge = document.querySelector('.cart-badge');
    if (badge) {
        badge.textContent = count;
        badge.style.display = count > 0 ? 'flex' : 'none';
    }
}

// Wishlist functionality
function toggleWishlist(productId) {
    const button = event.target;
    const isAdded = button.textContent === '♥';

    fetch('/Wishlist/Toggle', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ productId: productId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                if (data.isAdded) {
                    button.textContent = '♥';
                    button.style.color = 'var(--danger-color)';
                    showToast('به علاقه‌مندی‌ها اضافه شد');
                } else {
                    button.textContent = '♡';
                    button.style.color = '';
                    showToast('از علاقه‌مندی‌ها حذف شد');
                }
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('خطا در عملیات', 'error');
        });
}

// Quantity controls for cart
function updateQuantity(cartItemId, change) {
    const quantitySpan = event.target.parentNode.querySelector('span');
    let currentQuantity = parseInt(quantitySpan.textContent);
    let newQuantity = currentQuantity + change;

    if (newQuantity < 1) return;

    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({
            cartItemId: cartItemId,
            quantity: newQuantity
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                quantitySpan.textContent = newQuantity;
                updateCartTotal();
                updateCartBadge(data.cartCount);
            } else {
                showToast(data.message || 'خطا در به‌روزرسانی', 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('خطا در به‌روزرسانی', 'error');
        });
}

// Remove item from cart
function removeFromCart(cartItemId) {
    if (!confirm('آیا مطمئن هستید که می‌خواهید این محصول را حذف کنید؟')) {
        return;
    }

    fetch('/Cart/Remove', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ cartItemId: cartItemId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Remove the cart item from DOM
                const cartItem = event.target.closest('.cart-item');
                cartItem.remove();
                updateCartTotal();
                updateCartBadge(data.cartCount);
                showToast('محصول از سبد خرید حذف شد');
            } else {
                showToast(data.message || 'خطا در حذف محصول', 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('خطا در حذف محصول', 'error');
        });
}

// Update cart total
function updateCartTotal() {
    const cartItems = document.querySelectorAll('.cart-item');
    let total = 0;

    cartItems.forEach(item => {
        const priceElement = item.querySelector('.cart-item-info .price');
        const quantityElement = item.querySelector('.quantity-controls span');

        if (priceElement && quantityElement) {
            const price = parseFloat(priceElement.textContent.replace(/[^\d.-]/g, ''));
            const quantity = parseInt(quantityElement.textContent);
            total += price * quantity;
        }
    });

    const totalElement = document.querySelector('.cart-total h3');
    if (totalElement) {
        totalElement.textContent = `جمع کل: ${total.toLocaleString('fa-IR')} تومان`;
    }
}

// Search functionality
function initializeSearch() {
    const searchInput = document.querySelector('.search-box input');
    if (!searchInput) return;

    let searchTimeout;

    searchInput.addEventListener('input', function (e) {
        clearTimeout(searchTimeout);
        const searchTerm = e.target.value.trim();

        if (searchTerm.length >= 2) {
            searchTimeout = setTimeout(() => {
                performSearch(searchTerm);
            }, 500);
        }
    });
}

function performSearch(term) {
    fetch(`/Product/Search?term=${encodeURIComponent(term)}`)
        .then(response => response.json())
        .then(data => {
            // Update search results in UI
            updateSearchResults(data);
        })
        .catch(error => {
            console.error('Search error:', error);
        });
}

function updateSearchResults(results) {
    // This would update a search results dropdown or redirect to search page
    console.log('Search results:', results);
}

// Form validation
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (!form) return true;

    const requiredFields = form.querySelectorAll('[required]');
    let isValid = true;

    requiredFields.forEach(field => {
        if (!field.value.trim()) {
            field.style.borderColor = 'var(--danger-color)';
            isValid = false;
        } else {
            field.style.borderColor = 'var(--border-color)';
        }
    });

    return isValid;
}

// Checkout form submission
function submitCheckout() {
    if (!validateForm('checkout-form')) {
        showToast('لطفاً تمام فیلدهای الزامی را پر کنید', 'error');
        return;
    }

    const button = event.target;
    const originalText = button.textContent;

    // Show loading state
    button.innerHTML = '<div class="spinner" style="width: 20px; height: 20px; margin: 0;"></div>';
    button.disabled = true;

    const formData = new FormData(document.getElementById('checkout-form'));

    fetch('/Order/Checkout', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showToast('سفارش با موفقیت ثبت شد! 🎉');
                setTimeout(() => {
                    window.location.href = `/Order/Details/${data.orderId}`;
                }, 2000);
            } else {
                showToast(data.message || 'خطا در ثبت سفارش', 'error');
                button.textContent = originalText;
                button.disabled = false;
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('خطا در ثبت سفارش', 'error');
            button.textContent = originalText;
            button.disabled = false;
        });
}

// Admin tab switching
function showAdminTab(tabName) {
    // Hide all tab contents
    const tabContents = document.querySelectorAll('.admin-tab-content');
    tabContents.forEach(content => {
        content.style.display = 'none';
    });

    // Remove active class from all tabs
    const tabs = document.querySelectorAll('.nav-tab');
    tabs.forEach(tab => {
        tab.classList.remove('active');
    });

    // Show selected tab content
    const selectedContent = document.getElementById(tabName);
    if (selectedContent) {
        selectedContent.style.display = 'block';
    }

    // Add active class to clicked tab
    event.target.classList.add('active');
}

// Loading states
function showLoadingSkeletons(container) {
    const skeletons = container.querySelectorAll('.skeleton');
    skeletons.forEach(skeleton => {
        skeleton.classList.add('skeleton');
    });
}

function hideLoadingSkeletons(container) {
    const skeletons = container.querySelectorAll('.skeleton');
    skeletons.forEach(skeleton => {
        skeleton.classList.remove('skeleton');
    });
}

// Mobile menu toggle
function toggleMobileMenu() {
    const navLinks = document.querySelector('.nav-links');
    navLinks.classList.toggle('mobile-open');
}

// Initialize everything when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Initialize search
    initializeSearch();

    // Initialize wishlist buttons
    const wishlistBtns = document.querySelectorAll('.wishlist-btn');
    wishlistBtns.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            e.preventDefault();
            const productId = this.getAttribute('data-product-id');
            if (productId) {
                toggleWishlist(productId);
            }
        });
    });

    // Initialize quantity controls
    const quantityBtns = document.querySelectorAll('.quantity-btn');
    quantityBtns.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const cartItemId = this.getAttribute('data-cart-item-id');
            const change = this.textContent === '+' ? 1 : -1;
            if (cartItemId) {
                updateQuantity(cartItemId, change);
            }
        });
    });

    // Initialize remove buttons
    const removeBtns = document.querySelectorAll('.remove-btn');
    removeBtns.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const cartItemId = this.getAttribute('data-cart-item-id');
            if (cartItemId) {
                removeFromCart(cartItemId);
            }
        });
    });

    // Update cart total on page load
    updateCartTotal();

    // Smooth scrolling for anchor links
    const anchorLinks = document.querySelectorAll('a[href^="#"]');
    anchorLinks.forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth'
                });
            }
        });
    });
});

// Utility functions
function formatPrice(price) {
    return new Intl.NumberFormat('fa-IR').format(price) + ' تومان';
}

function formatDate(dateString) {
    const options = {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        calendar: 'persian'
    };
    return new Intl.DateTimeFormat('fa-IR', options).format(new Date(dateString));
}

// Product image lazy loading
function initLazyLoading() {
    const images = document.querySelectorAll('img[data-src]');

    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                imageObserver.unobserve(img);
            }
        });
    });

    images.forEach(img => imageObserver.observe(img));
}

// Initialize lazy loading when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initLazyLoading);
} else {
    initLazyLoading();
}