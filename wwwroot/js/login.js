// Enhanced form interactions
document.addEventListener('DOMContentLoaded', function () {
    const inputs = document.querySelectorAll('input[type="email"], input[type="password"]');

    inputs.forEach(input => {
        input.addEventListener('focus', function () {
            this.closest('.input-group').classList.add('focused');
        });

        input.addEventListener('blur', function () {
            this.closest('.input-group').classList.remove('focused');
        });

        // Add floating label effect
        input.addEventListener('input', function () {
            if (this.value) {
                this.classList.add('has-value');
            } else {
                this.classList.remove('has-value');
            }
        });
    });

    // Add subtle hover effects to the form
    const form = document.querySelector('form');
    form.addEventListener('mouseenter', function () {
        this.style.transform = 'translateY(-2px)';
    });

    form.addEventListener('mouseleave', function () {
        this.style.transform = 'translateY(0)';
    });
});