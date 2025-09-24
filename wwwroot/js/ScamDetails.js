// Scam Details Page Functionality
class ScamDetails {
    constructor() {
        this.currentImageIndex = 0;
        this.evidenceImages = [];
        this.init();
    }

    init() {
        this.setupEvidenceViewer();
        this.setupVoteButtons();
        this.setupActionButtons();
        this.setupImageModal();
        this.setupAccessibility();
    }

    // Evidence image viewer functionality
    setupEvidenceViewer() {
        this.evidenceItems = document.querySelectorAll('.evidence-item');
        this.evidenceImages = Array.from(this.evidenceItems).map(item =>
            item.getAttribute('data-image') || item.querySelector('img').src
        );

        this.evidenceItems.forEach((item, index) => {
            // Click on image
            item.addEventListener('click', (e) => {
                if (!e.target.closest('.evidence-zoom')) {
                    this.openImageModal(index);
                }
            });

            // Click on zoom button
            const zoomBtn = item.querySelector('.evidence-zoom');
            if (zoomBtn) {
                zoomBtn.addEventListener('click', (e) => {
                    e.stopPropagation();
                    this.openImageModal(index);
                });
            }

            // Keyboard navigation
            item.setAttribute('tabindex', '0');
            item.setAttribute('role', 'button');
            item.setAttribute('aria-label', `View evidence image ${index + 1}`);

            item.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    this.openImageModal(index);
                }
            });
        });
    }

    // Image modal functionality
    setupImageModal() {
        this.modal = document.getElementById('imageModal');
        this.modalImage = document.getElementById('modalImage');
        this.imageCounter = document.getElementById('imageCounter');
        this.modalClose = document.getElementById('modalClose');
        this.prevButton = document.getElementById('prevImage');
        this.nextButton = document.getElementById('nextImage');

        if (!this.modal) return;

        // Close modal
        this.modalClose.addEventListener('click', () => this.closeImageModal());

        // Previous/next navigation
        this.prevButton.addEventListener('click', () => this.showPreviousImage());
        this.nextButton.addEventListener('click', () => this.showNextImage());

        // Keyboard navigation
        document.addEventListener('keydown', (e) => {
            if (this.modal.classList.contains('active')) {
                switch (e.key) {
                    case 'Escape':
                        this.closeImageModal();
                        break;
                    case 'ArrowLeft':
                        this.showPreviousImage();
                        break;
                    case 'ArrowRight':
                        this.showNextImage();
                        break;
                }
            }
        });

        // Click outside to close
        this.modal.addEventListener('click', (e) => {
            if (e.target === this.modal) {
                this.closeImageModal();
            }
        });
    }

    openImageModal(index) {
        if (this.evidenceImages.length === 0) return;

        this.currentImageIndex = index;
        this.updateModalImage();
        this.modal.classList.add('active');
        document.body.style.overflow = 'hidden';

        // Set focus for accessibility
        setTimeout(() => {
            this.modalClose.focus();
        }, 100);
    }

    closeImageModal() {
        this.modal.classList.remove('active');
        document.body.style.overflow = '';

        // Return focus to the evidence item that was clicked
        if (this.evidenceItems[this.currentImageIndex]) {
            this.evidenceItems[this.currentImageIndex].focus();
        }
    }

    showPreviousImage() {
        this.currentImageIndex = (this.currentImageIndex - 1 + this.evidenceImages.length) % this.evidenceImages.length;
        this.updateModalImage();
    }

    showNextImage() {
        this.currentImageIndex = (this.currentImageIndex + 1) % this.evidenceImages.length;
        this.updateModalImage();
    }

    updateModalImage() {
        if (!this.modalImage || !this.imageCounter) return;

        this.modalImage.src = this.evidenceImages[this.currentImageIndex];
        this.imageCounter.textContent = `${this.currentImageIndex + 1} / ${this.evidenceImages.length}`;

        // Update button states
        this.prevButton.disabled = this.evidenceImages.length <= 1;
        this.nextButton.disabled = this.evidenceImages.length <= 1;

        // Preload adjacent images for smoother navigation
        this.preloadAdjacentImages();
    }

    preloadAdjacentImages() {
        const preloadIndices = [
            (this.currentImageIndex - 1 + this.evidenceImages.length) % this.evidenceImages.length,
            (this.currentImageIndex + 1) % this.evidenceImages.length
        ];

        preloadIndices.forEach(index => {
            const img = new Image();
            img.src = this.evidenceImages[index];
        });
    }

    // Vote button functionality
    setupVoteButtons() {
        const voteForms = document.querySelectorAll('.vote-form');

        voteForms.forEach(form => {
            form.addEventListener('submit', (e) => {
                const button = form.querySelector('.vote-btn');
                if (button) {
                    this.handleVoteSubmission(button);
                }
            });
        });
    }

    handleVoteSubmission(button) {
        // Add loading state
        const originalText = button.innerHTML;
        button.disabled = true;
        button.classList.add('loading');
        button.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i> Voting...';

        // Simulate API call delay
        setTimeout(() => {
            button.disabled = false;
            button.classList.remove('loading');
            button.innerHTML = originalText;

            // Show success message
            this.showToast('Vote submitted successfully!', 'success');
        }, 1000);
    }

    // Action buttons functionality
    setupActionButtons() {
        // Report issue button
        const reportBtn = document.getElementById('reportIssueBtn');
        if (reportBtn) {
            reportBtn.addEventListener('click', () => {
                this.showReportModal();
            });
        }

        // Share alert button
        const shareBtn = document.getElementById('shareAlertBtn');
        if (shareBtn) {
            shareBtn.addEventListener('click', () => {
                this.shareScamAlert();
            });
        }
    }

    showReportModal() {
        // Create and show report modal
        const modalHtml = `
            <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
                <div class="bg-white dark:bg-gray-800 rounded-xl p-6 mx-4 max-w-md w-full">
                    <h3 class="text-lg font-bold mb-4 text-gray-900 dark:text-white">Report Issue</h3>
                    <p class="text-gray-600 dark:text-gray-300 mb-4">What issue would you like to report?</p>
                    <div class="space-y-3">
                        <label class="flex items-center p-3 rounded-lg border border-gray-200 dark:border-gray-600 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700">
                            <input type="radio" name="issue" value="inaccurate" class="mr-3">
                            <span class="text-gray-700 dark:text-gray-200">Inaccurate Information</span>
                        </label>
                        <label class="flex items-center p-3 rounded-lg border border-gray-200 dark:border-gray-600 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700">
                            <input type="radio" name="issue" value="duplicate" class="mr-3">
                            <span class="text-gray-700 dark:text-gray-200">Duplicate Scam Report</span>
                        </label>
                        <label class="flex items-center p-3 rounded-lg border border-gray-200 dark:border-gray-600 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700">
                            <input type="radio" name="issue" value="other" class="mr-3">
                            <span class="text-gray-700 dark:text-gray-200">Other Issue</span>
                        </label>
                    </div>
                    <div class="mt-6 flex justify-end space-x-3">
                        <button class="px-4 py-2 text-gray-600 dark:text-gray-300 hover:text-gray-800 dark:hover:text-gray-100 transition-colors cancel-btn">Cancel</button>
                        <button class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors submit-btn">Submit Report</button>
                    </div>
                </div>
            </div>
        `;

        const modal = document.createElement('div');
        modal.innerHTML = modalHtml;
        document.body.appendChild(modal);

        const cancelBtn = modal.querySelector('.cancel-btn');
        const submitBtn = modal.querySelector('.submit-btn');

        cancelBtn.addEventListener('click', () => {
            document.body.removeChild(modal);
        });

        submitBtn.addEventListener('click', () => {
            this.submitReport(modal);
        });

        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                document.body.removeChild(modal);
            }
        });
    }

    submitReport(modal) {
        const selectedIssue = modal.querySelector('input[name="issue"]:checked');
        if (!selectedIssue) {
            this.showToast('Please select an issue type', 'error');
            return;
        }

        const submitBtn = modal.querySelector('.submit-btn');
        submitBtn.disabled = true;
        submitBtn.textContent = 'Submitting...';

        // Simulate API call
        setTimeout(() => {
            document.body.removeChild(modal);
            this.showToast('Report submitted successfully!', 'success');
        }, 1500);
    }

    shareScamAlert() {
        const shareData = {
            title: document.title,
            text: 'Check out this scam alert on Scam Sentinel',
            url: window.location.href
        };

        if (navigator.share) {
            navigator.share(shareData).catch(err => {
                console.log('Error sharing:', err);
                this.copyToClipboard();
            });
        } else {
            this.copyToClipboard();
        }
    }

    copyToClipboard() {
        navigator.clipboard.writeText(window.location.href).then(() => {
            this.showToast('Link copied to clipboard!', 'success');
        }).catch(err => {
            console.log('Error copying to clipboard:', err);
            this.showToast('Failed to copy link', 'error');
        });
    }

    // Accessibility improvements
    setupAccessibility() {
        // Add aria-live region for dynamic content
        const liveRegion = document.createElement('div');
        liveRegion.setAttribute('aria-live', 'polite');
        liveRegion.setAttribute('aria-atomic', 'true');
        liveRegion.className = 'sr-only';
        document.body.appendChild(liveRegion);

        this.liveRegion = liveRegion;
    }

    // Toast notification system
    showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `fixed top-4 right-4 z-50 px-6 py-3 rounded-lg text-white font-medium shadow-lg transform translate-x-full transition-transform duration-300 ${type === 'success' ? 'bg-green-500' :
                type === 'error' ? 'bg-red-500' : 'bg-blue-500'
            }`;
        toast.textContent = message;

        document.body.appendChild(toast);

        // Animate in
        requestAnimationFrame(() => {
            toast.style.transform = 'translateX(0)';
        });

        // Announce to screen readers
        this.liveRegion.textContent = message;

        // Auto remove after 3 seconds
        setTimeout(() => {
            toast.style.transform = 'translateX(100%)';
            setTimeout(() => {
                if (toast.parentNode) {
                    document.body.removeChild(toast);
                }
            }, 300);
        }, 3000);
    }

    // Error handling for images
    handleImageError(img) {
        img.src = '/images/placeholder-image.jpg';
        img.alt = 'Image not available';
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ScamDetails();
});

// Add error handlers for all images
document.addEventListener('DOMContentLoaded', () => {
    const images = document.querySelectorAll('img');
    images.forEach(img => {
        img.addEventListener('error', function () {
            this.src = '/images/placeholder-image.jpg';
            this.alt = 'Image not available';
        });
    });
});

// Screen reader only class
const style = document.createElement('style');
style.textContent = `
    .sr-only {
        position: absolute;
        width: 1px;
        height: 1px;
        padding: 0;
        margin: -1px;
        overflow: hidden;
        clip: rect(0, 0, 0, 0);
        white-space: nowrap;
        border: 0;
    }
`;
document.head.appendChild(style);
// Character counter for comment textarea
document.addEventListener('DOMContentLoaded', function () {
    // New comment character counter
    const commentTextarea = document.querySelector('.comment-textarea');
    const charCount = document.querySelector('.char-count');

    if (commentTextarea && charCount) {
        commentTextarea.addEventListener('input', function () {
            charCount.textContent = this.value.length;
        });
    }

    // Edit comment functionality
    document.querySelectorAll('.comment-edit-btn').forEach(button => {
        button.addEventListener('click', function () {
            const commentId = this.getAttribute('data-comment-id');
            const editForm = document.getElementById(`edit-form-${commentId}`);
            const commentText = this.closest('.comment-content').querySelector('.comment-text');

            // Hide comment text and show edit form
            commentText.style.display = 'none';
            this.style.display = 'none';
            editForm.style.display = 'block';

            // Set up character counter for edit form
            const editTextarea = editForm.querySelector('.edit-comment-textarea');
            const editCharCount = editForm.querySelector('.edit-char-count');

            editTextarea.addEventListener('input', function () {
                editCharCount.textContent = this.value.length;
            });
        });
    });

    // Cancel edit functionality
    document.querySelectorAll('.cancel-edit-btn').forEach(button => {
        button.addEventListener('click', function () {
            const commentId = this.getAttribute('data-comment-id');
            const editForm = document.getElementById(`edit-form-${commentId}`);
            const commentText = editForm.closest('.comment-content').querySelector('.comment-text');
            const editButton = editForm.closest('.comment-content').querySelector('.comment-edit-btn');

            // Show comment text and edit button, hide edit form
            commentText.style.display = 'block';
            editButton.style.display = 'block';
            editForm.style.display = 'none';
        });
    });

    // Form validation
    document.querySelectorAll('.comment-form, .edit-comment-form').forEach(form => {
        form.addEventListener('submit', function (e) {
            const textarea = this.querySelector('textarea[name="CommentText"], textarea[name="commentText"]');
            if (textarea && textarea.value.trim().length === 0) {
                e.preventDefault();
                alert('Please enter a comment before submitting.');
                textarea.focus();
            }
        });
    });
});
document.querySelectorAll('.comment-edit-btn').forEach(button => {
    button.addEventListener('click', function () {
        const commentId = this.getAttribute('data-comment-id');
        const editForm = document.getElementById(`edit-form-${commentId}`);
        editForm.classList.remove('hidden');
    });
});

// Cancel edit
document.querySelectorAll('.cancel-edit-btn').forEach(button => {
    button.addEventListener('click', function () {
        const commentId = this.getAttribute('data-comment-id');
        const editForm = document.getElementById(`edit-form-${commentId}`);
        editForm.classList.add('hidden');
    });
});