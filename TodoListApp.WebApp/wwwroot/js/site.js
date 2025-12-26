// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Real-time task search functionality with tag filtering
(function() {
    document.addEventListener('DOMContentLoaded', function() {
        const searchInput = document.getElementById('searchTermInput');
        const searchResults = document.getElementById('searchResults');
        const tagFilterContainer = document.getElementById('tagFilterContainer');
        const searchDropdown = document.getElementById('searchDropdown');
        let searchTimeout;
        let selectedTags = new Set();
        let allTags = [];

        // Load tags when dropdown is opened
        if (searchDropdown && tagFilterContainer) {
            searchDropdown.addEventListener('shown.bs.dropdown', function() {
                if (allTags.length === 0) {
                    loadTags();
                }
            });
        }

        function loadTags() {
            fetch('/TodoTasks/GetTagsJson')
                .then(response => response.json())
                .then(data => {
                    allTags = data.tags || [];
                    renderTags();
                })
                .catch(error => {
                    console.error('Error loading tags:', error);
                    tagFilterContainer.innerHTML = '<div class="text-muted small">Error loading tags.</div>';
                });
        }

        function renderTags() {
            if (allTags.length === 0) {
                tagFilterContainer.innerHTML = '<div class="text-muted small">No tags available.</div>';
                return;
            }

            let html = '';
            allTags.forEach(function(tag) {
                const isSelected = selectedTags.has(tag.id);
                html += `<button type="button" 
                    class="tag-filter-btn ${isSelected ? 'tag-filter-selected' : ''}" 
                    data-tag-id="${tag.id}"
                    onclick="toggleSearchTag(${tag.id})"
                    style="font-size: 0.75rem; padding: 0.25rem 0.5rem;">
                    ${escapeHtml(tag.name)}
                </button>`;
            });
            tagFilterContainer.innerHTML = html;
        }

        window.toggleSearchTag = function(tagId) {
            const btn = document.querySelector(`#tagFilterContainer [data-tag-id="${tagId}"]`);
            if (selectedTags.has(tagId)) {
                selectedTags.delete(tagId);
                btn.classList.remove('tag-filter-selected');
            } else {
                selectedTags.add(tagId);
                btn.classList.add('tag-filter-selected');
            }
            performSearch();
        };

        function performSearch() {
            const searchTerm = searchInput ? searchInput.value.trim() : '';
            
            clearTimeout(searchTimeout);
            
            // Show results if there's a search term or selected tags
            if (searchTerm.length === 0 && selectedTags.size === 0) {
                searchResults.innerHTML = '<div class="px-3 py-2 text-muted small">Start typing to search or select tags...</div>';
                return;
            }

            if (searchTerm.length > 0 && searchTerm.length < 2) {
                if (selectedTags.size === 0) {
                    searchResults.innerHTML = '<div class="px-3 py-2 text-muted small">Type at least 2 characters...</div>';
                    return;
                }
            }

            // Debounce search
            searchTimeout = setTimeout(function() {
                let searchUrl = '/TodoTasks/SearchJson?';
                if (searchTerm.length > 0) {
                    searchUrl += 'searchTerm=' + encodeURIComponent(searchTerm);
                }
                if (selectedTags.size > 0) {
                    const tagIds = Array.from(selectedTags).join('&tagIds=');
                    if (searchTerm.length > 0) {
                        searchUrl += '&';
                    }
                    searchUrl += 'tagIds=' + tagIds;
                }

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
        }

        if (searchInput && searchResults) {
            searchInput.addEventListener('input', function() {
                performSearch();
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

            // Prevent dropdown from closing when clicking tag buttons
            if (tagFilterContainer) {
                tagFilterContainer.addEventListener('click', function(e) {
                    if (e.target.classList.contains('tag-filter-btn')) {
                        e.stopPropagation();
                    }
                });
            }
        }

        function escapeHtml(text) {
            if (!text) return '';
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }
    });
})();