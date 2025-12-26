// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Real-time task search functionality
(function() {
    document.addEventListener('DOMContentLoaded', function() {
        const searchInput = document.getElementById('searchTermInput');
        const searchResults = document.getElementById('searchResults');
        let searchTimeout;

        if (searchInput && searchResults) {
            searchInput.addEventListener('input', function() {
                const searchTerm = this.value.trim();
                
                clearTimeout(searchTimeout);
                
                if (searchTerm.length === 0) {
                    searchResults.innerHTML = '<div class="px-3 py-2 text-muted small">Start typing to search...</div>';
                    return;
                }

                if (searchTerm.length < 2) {
                    searchResults.innerHTML = '<div class="px-3 py-2 text-muted small">Type at least 2 characters...</div>';
                    return;
                }

                // Debounce search
                searchTimeout = setTimeout(function() {
                    const searchUrl = '/TodoTasks/SearchJson?searchTerm=' + encodeURIComponent(searchTerm);
                    fetch(searchUrl)
                        .then(response => response.json())
                        .then(data => {
                            if (data.tasks && data.tasks.length > 0) {
                                let html = '';
                                data.tasks.forEach(function(task) {
                                    const overdueClass = task.isOverdue ? 'text-danger' : '';
                                    const overdueBadge = task.isOverdue ? '<span class="badge bg-danger ms-2">Overdue</span>' : '';
                                    const detailsUrl = '/TodoTasks/Details/' + task.id;
                                    html += `
                                        <a href="${detailsUrl}" class="dropdown-item ${overdueClass}" style="white-space: normal;">
                                            <div class="fw-bold">${escapeHtml(task.title)}${overdueBadge}</div>
                                            <div class="small text-muted">
                                                ${task.todoListTitle ? 'List: ' + escapeHtml(task.todoListTitle) + ' • ' : ''}
                                                Status: ${task.status}
                                                ${task.dueDate ? ' • Due: ' + task.dueDate : ''}
                                            </div>
                                        </a>
                                    `;
                                });
                                searchResults.innerHTML = html;
                            } else {
                                searchResults.innerHTML = '<div class="px-3 py-2 text-muted small">No tasks found.</div>';
                            }
                        })
                        .catch(error => {
                            console.error('Search error:', error);
                            searchResults.innerHTML = '<div class="px-3 py-2 text-danger small">Error searching tasks.</div>';
                        });
                }, 300);
            });

            // Keep dropdown open when clicking inside
            searchInput.addEventListener('click', function(e) {
                e.stopPropagation();
            });

            // Prevent dropdown from closing when clicking results (except links)
            searchResults.addEventListener('click', function(e) {
                if (!e.target.closest('a')) {
                    e.stopPropagation();
                }
            });
        }

        function escapeHtml(text) {
            if (!text) return '';
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }
    });
})();