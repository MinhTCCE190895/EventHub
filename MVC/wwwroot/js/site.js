(() => {
    "use strict";

    document.addEventListener("DOMContentLoaded", () => {
        document.querySelector("[data-sidebar-toggle]")?.addEventListener("click", () => {
            document.querySelector(".app-sidebar")?.classList.toggle("show");
        });

        document.querySelectorAll("form[data-confirm]").forEach(form => {
            form.addEventListener("submit", event => {
                const message = form.dataset.confirm;
                if (message && !window.confirm(message)) {
                    event.preventDefault();
                }
            });
        });

        document.querySelectorAll("[data-auto-dismiss='true']").forEach(alertElement => {
            window.setTimeout(() => {
                if (window.bootstrap?.Alert) {
                    window.bootstrap.Alert.getOrCreateInstance(alertElement).close();
                }
            }, 4000);
        });

        const roleSelect = document.querySelector("[data-role-select]");
        const studentCodeGroup = document.querySelector("[data-student-code-group]");
        if (roleSelect && studentCodeGroup) {
            const updateStudentCodeVisibility = () => {
                const isStudent = roleSelect.value === roleSelect.dataset.studentRole;
                studentCodeGroup.classList.toggle("d-none", !isStudent);
            };

            roleSelect.addEventListener("change", updateStudentCodeVisibility);
            updateStudentCodeVisibility();
        }

        const modalElement = document.querySelector("[data-show-on-load='true']");
        if (modalElement && window.bootstrap?.Modal) {
            window.bootstrap.Modal.getOrCreateInstance(modalElement).show();
        }

        const locale = document.documentElement.lang || "vi-VN";
        document.querySelectorAll("time[data-local-datetime]").forEach(timeElement => {
            const date = new Date(timeElement.dateTime);
            if (Number.isNaN(date.getTime())) {
                return;
            }

            const options = timeElement.dataset.dateOnly === "true"
                ? { year: "numeric", month: "2-digit", day: "2-digit" }
                : {
                    year: "numeric",
                    month: "2-digit",
                    day: "2-digit",
                    hour: "2-digit",
                    minute: "2-digit"
                };
            timeElement.textContent = new Intl.DateTimeFormat(locale, options).format(date);
        });
    });
})();
