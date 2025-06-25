<template>
    <div class="d-flex flex-column" style="height: 100vh !important;">
        <router-view :key="$route.fullPath" />
        <!-- Component ToastAlert -->
        <toast-notification :showToast="toastShow" @close="closeToast" />
    </div>
</template>

<script>
    import GlobalEventService from './services/globalEventService';
    import ToastNotification from '@/components/common/toast-notification';
    export default {
        name: 'App',
        data() {
            return {
                title: "App",
                toastShow: false,
                toastColor: "",
                toastMessage: "",
            }
        },
        components:
        {
            ToastNotification,
        },
        methods: {
            handleUploadComplete(payload) {
            },
            handleUploadInProgress(payload) {
            },
            handleUploadStarted(payload) {
                this.alertToast();
            },
            alertToast: function (msg, color) {
                this.toastShow = true;
            },
            closeToast: function () {
                this.toastShow = false;
            },
        },
        created() {
            GlobalEventService.on('uploadInProgress', this.handleUploadInProgress);
            GlobalEventService.on('uploadComplete', this.handleUploadComplete);
            GlobalEventService.on('uploadStarted', this.handleUploadStarted);

        },
    }
</script>

<style>
    /* CSS Themes */
    .css-theme-light {
        --color-bg-sidebar-content: #ffffff;
        --color-sidebar-li: rgba(0, 0, 0, .85);
        --color-sidebar-li-collapsed: #212529;
        --color-sidebar-li-collapsed-hover: #212529;
        --color-sidebar-li-collapsed-active: #212529;
        --color-bg-sidebar-li-collapsed-active: #d2f4ea;
        --color-sidebar-dropdown: #212529;
        --color-bg-body-content: #f5f5f5;
        --color-body-content: #212529;
        --color-card-content: #ffffff;
        --color-card-title: #212529;
        --color-card-text: #212529;
        --color-bg-btn-primary: #0d6efd;
        --color-btn-primary: #ffffff;
        --color-bg-btn-danger: #dc3545;
        --color-bg-btn-success: #0EAA42;
        --color-bg-btn-secondary: #b7b7b7;
        --color-bg-form-control: #ffffff;
        --color-multiselect-option: #000000;
        --color-border-form-control: #0073E6;
        --color-multiselect-tag: #10b981;
        --color-h5-custom-modal: #0073E6;
        --color-span-muted: #5E5873;
        --color-bg-btn-cancel: #F0F7FF;
        --color-btn-cancel: #0073E6;
        --color-bg-table-tr-first: #f3f3f3;
        --color-fa-list-doc: #0d6efd;
        --color-bg-page-link: #f5f5f5;
        --color-bg-page-link-active: #0d6efd;
        --color-bg-icon-active: #0d6efd;
        --color-page-link: #0d6efd;
        --color-page-link-active: #ffffff;
        --color-page-link-disabled: #6c757d;
        --color-border-page-link: 1px solid #dee2e6;
        --color-border-btn-outline-primary: #0d6efd;
        --color-btn-outline-primary: #0d6efd;
        --filter-btn-close: initial;
        --color-bg-toast-content: #ffffff;
        --color-bg-toast-content-primary: #cfe2ff;
        --color-bg-toast-content-upload: #E3EBF6;
        --color-bg-toast-content-success: #d1e7dd;
        --color-bg-toast-content-warning: #fff3cd;
        --color-bg-toast-content-danger: #f8d7da;
        --color-bg-toast: rgba(255,255,255,.85);
        --color-toast-content: #6c757d;
        --color-toast-content-primary: #084298;
        --color-toast-content-success: #0f5132;
        --color-toast-content-warning: #664d03;
        --color-toast-content-danger: #842029;
        --color-border-toast: 1px solid rgba(0,0,0,.05);
        --color-border-header-modal: 1px solid #dee2e6;
        --box-shadow-multiselect-wrapper: 0 0 0 var(--ms-ring-width, 3px) var(--ms-ring-color, rgba(16, 185, 129, .188));
        --color-bg-dropdown-menu: #ffffff;
        --color-dropdown-menu: #212529;
        --color-fade-buttons: #d2f4ea;
        --color-table-striped: #FFFF;
        --color-table-accent: #F3F4F6;
        --color-bg-form-outline: #0D6EFD;
        --color-bg-table-outline: #E7E8E9;
        --image-chevron: url("data:image/svg+xml,%3csvg xmlns=%27http://www.w3.org/2000/svg%27 viewBox=%270 0 16 16%27%3e%3cpath fill=%27none%27 stroke=%27%237e7e7e%27 stroke-linecap=%27round%27 stroke-linejoin=%27round%27 stroke-width=%272%27 d=%27M2 5l6 6 6-6%27/%3e%3c/svg%3e");
        --color-bg-nav-button: #F0F6FF;
        --color-icon-nav-button: #0D6EFD;
        --icon-theme: "\f186";
    }

    .css-theme-dark {
        --color-bg-sidebar-content: #0B1326;
        --color-sidebar-li: #B6C2CF;
        --color-sidebar-li-collapsed: #ffffff;
        --color-sidebar-li-collapsed-hover: #ffffff;
        --color-sidebar-li-collapsed-active: #ffffff;
        --color-bg-sidebar-li-collapsed-active: #5b5b5b;
        --color-bg-sidebar-hr: #ffffff;
        --color-sidebar-dropdown: #ffffff;
        --color-bg-body-content: #212936;
        --color-body-content: #B6C2CF;
        --color-card-content: #282F3B;
        --color-card-title: #B6C2CF;
        --color-card-text: #B6C2CF;
        --color-fa-list-doc: #CC3333;
        --color-bg-btn-primary: #1C3C6E;
        --color-btn-primary: #ffffff;
        --color-bg-btn-danger: #bb4d58;
        --color-bg-btn-success: #196C3D;
        --color-bg-btn-secondary: #8d8d8d;
        --color-bg-form-control: #121826;
        --color-border-form-control: #0073E6;
        --color-multiselect-option: #000000;
        --color-multiselect-tag: #555555;
        --color-h5-custom-modal: #B6C2CF;
        --color-span-muted: #e1e1e1;
        --color-bg-btn-cancel: #A9AFBD;
        --color-btn-cancel: #212529;
        --color-bg-table-tr-first: #0B1326;
        --color-td-table: #1E2531;
        --color-bg-page-link: #212936;
        --color-bg-page-link-active: #121826;
        --color-bg-icon-active: #0d6efd;
        --color-page-link: #d0d0d0;
        --color-page-link-active: #ffffff;
        --color-page-link-disabled: #4f4f4f;
        --color-border-page-link: 1px solid #4f4f4f;
        --color-border-btn-outline-primary: #0D6EFD;
        --color-btn-outline-primary: #0D6EFD;
        --filter-btn-close: invert(1) grayscale(100%) brightness(200%);
        --color-bg-toast-content: #212936;
        --color-bg-toast-content-primary: #1C3C6E;
        --color-bg-toast-content-upload: #1E2531;
        --color-bg-toast-content-warning: #FBCC92;
        --color-bg-toast-content-danger: #CC3333;
        --color-bg-toast-content-success: #196C3D;
        --color-bg-toast: #212936;
        --color-toast-content: #ffffff;
        --color-toast-content-primary: #FFFFFF;
        --color-toast-content-success: #FFFFFF;
        --color-toast-content-warning: #212936;
        --color-toast-content-danger: #FFFFFF;
        --color-border-toast: 1px solid #212936;
        --color-border-header-modal: 1px solid #4b4b4b;
        --color-table-striped: #282F3B;
        --box-shadow-multiselect-wrapper: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
        --color-bg-dropdown-menu: #0B1326;
        --color-dropdown-menu: #ffffff;
        --color-fade-buttons: #5a8b94;
        --content-chevron: invert(100%);
        --color-table-accent: #1E2531;
        --color-upload-icon: #0D6EFD;
        --color-bg-form-outline: #0D6EFD;
        --color-bg-table-outline: #0B1326;
        --image-chevron: url("data:image/svg+xml,%3csvg xmlns=%27http://www.w3.org/2000/svg%27 viewBox=%270 0 16 16%27%3e%3cpath fill=%27none%27 stroke=%27%23ffffff%27 stroke-linecap=%27round%27 stroke-linejoin=%27round%27 stroke-width=%272%27 d=%27M2 5l6 6 6-6%27/%3e%3c/svg%3e");
        --color-bg-nav-button: #282F3B;
        --color-icon-nav-button: #9095A1;
        --icon-theme: "\f0a3";
    }

    html {
        height: -webkit-fill-available;
    }

    body {
        background-color: var(--color-bg-body-content) !important;
        color: var(--color-body-content) !important;
        min-height: 100vh;
        min-height: -webkit-fill-available;
    }

    html, body {
        max-width: 100%;
        overflow-x: hidden;
    }

        body .overlay {
            display: none;
            width: 100%;
            height: 100%;
            background: rgba(0,0,0,.85);
            position: fixed;
            left: 0;
            top: 0;
            z-index: -1;
        }

            body .overlay.active {
                display: block;
                z-index: 1;
            }

    main {
        display: flex;
        flex-wrap: nowrap;
        height: 100vh;
        height: -webkit-fill-available;
        max-height: 100vh;
        overflow-x: auto;
        overflow-y: hidden;
    }

        main > .initial-sidebar,
        main > #offcanvas-sidebar {
            background: var(--color-bg-sidebar-content) !important;
        }

            main > .initial-sidebar > ul > li > a,
            main > #offcanvas-sidebar > ul > li > a {
                color: var(--color-sidebar-li) !important;
            }

                main > .initial-sidebar > ul > li > a:hover,
                main > #offcanvas-sidebar > ul > li > a:hover {
                    background-color: var(--color-bg-sidebar-li-collapsed-active) !important;
                }

            main > .initial-sidebar > ul > li > .collapsed > ul > li > a,
            main > #offcanvas-sidebar > ul > li > .collapsed > ul > li > a {
                color: var(--color-sidebar-li-collapsed) !important;
            }

                main > .initial-sidebar > ul > li > .collapsed > ul > li > a:hover,
                main > #offcanvas-sidebar > ul > li > .collapsed > ul > li > a:hover {
                    color: var(--color-sidebar-li-collapsed-hover) !important;
                    background-color: var(--color-bg-sidebar-li-collapsed-active) !important;
                }

            main > .initial-sidebar > ul > li > .collapsed > ul > li > .active,
            main > #offcanvas-sidebar > ul > li > .collapsed > ul > li > .active {
                color: var(--color-sidebar-li-collapsed-active) !important;
                background-color: var(--color-bg-sidebar-li-collapsed-active) !important;
            }

            main > .initial-sidebar > hr,
            main > #offcanvas-sidebar > hr {
                background-color: var(--color-bg-sidebar-hr) !important;
            }

            main > .initial-sidebar > .dropdown > a,
            main > #offcanvas-sidebar > .dropdown > a {
                color: var(--color-sidebar-dropdown) !important;
            }

    .input-group:not(.has-validation) > .dropdown-toggle:nth-last-child(n+3), .input-group:not(.has-validation) >
    :not(:last-child):not(.dropdown-toggle):not(.dropdown-menu):not(.input-group-text) {
        background: var(--color-bg-form-control) !important;
        color: var(--color-body-content) !important;
    }

    .dropdown-menu-sidebar {
        background-color: var(--color-bg-dropdown-menu) !important;
    }

        .dropdown-menu-sidebar > .remove-hover > .dropdown-item {
            background-color: var(--color-bg-dropdown-menu) !important;
            color: var(--color-dropdown-menu) !important;
        }

    .btn.btn-light.lang-link {
        color: var(--color-dropdown-menu) !important;
    }

    .btn-light {
        background-color: var(--color-bg-dropdown-menu) !important;
        color: var(--color-dropdown-menu) !important;
        border-color: var(--color-bg-dropdown-menu) !important;
    }

    main > .container-fluid {
        /*padding: 15px 10px 15px 0px;*/
        padding: 15px 15px 0px 0px;
        background: var(--color-bg-body-content) !important;
    }

        main > .container-fluid > .custom-padding > .row > .col-auto > .card {
            background: var(--color-card-content) !important;
        }

            main > .container-fluid > .custom-padding > .row > .col-auto > .card > .card-body > .card-title {
                color: var(--color-card-title) !important;
            }

            main > .container-fluid > .custom-padding > .row > .col-auto > .card > .card-body > .card-text {
                color: var(--color-card-text) !important;
            }

        main > .container-fluid > .custom-padding > .row > .col-auto > nav > ul > .active > .page-link {
            background-color: var(--color-bg-page-link-active) !important;
            border-color: var(--color-bg-page-link-active) !important;
            color: var(--color-page-link-active) !important;
            border-radius: 9px !important;
        }

        main > .container-fluid > .custom-padding > .row > .col-auto > nav > ul > li > .page-link {
            background-color: var(--color-bg-page-link) !important;
            border: var(--color-bg-page-link) !important;
            color: var(--color-page-link) !important;
        }

        main > .container-fluid > .custom-padding > .row > .col-auto > nav > ul > .disabled > .page-link {
            color: var(--color-page-link-disabled) !important;
        }

        main > .container-fluid > .custom-padding > .row > .table-responsive > table > tbody {
            background-color: var(--color-bg-table-tr-first) !important;
            border-color: var(--color-bg-table-tr-first) !important;
        }

            main > .container-fluid > .custom-padding > .row > .table-responsive > table > tbody > {
                background-color: var(--color-table-striped) !important;
            }

        main > .container-fluid > .custom-padding > .row > .table-responsive > table > tbody {
            background-color: var(--color-table-striped) !important;
            border-color: var(--color-table-striped) !important;
        }

    .table-striped {
        border-color: var(--color-bg-table-tr-first) !important;
        border-radius: 9px !important;
        overflow: hidden;
        color: var(--color-body-content) !important;
    }

        .table-striped > tbody > tr:nth-of-type(odd):not(:first-child) {
            --bs-table-accent-bg: var(--color-table-accent) !important;
        }

        .table-striped > tbody > tr:nth-of-type(odd) {
            color: var(--color-body-content) !important;
        }

        .table-striped > trhead {
            background-color: var(--color-bg-table-tr-first) !important;
            border-color: var(--color-bg-table-tr-first) !important;
        }

    .form-control {
        background-color: var(--color-bg-form-control) !important;
        border-color: var(--color-border-form-control) !important;
        color: var(--color-body-content) !important;
    }

    .custom-textarea {
        border-color: var(--color-bg-form-outline) !important;
    }

    .form-select {
        background-color: var(--color-bg-form-control) !important;
        color: var(--color-body-content) !important;
        border-color: var(--color-border-form-control) !important;
        background-image: var(--image-chevron) !important;
    }

    .multiselect, .multiselect-tags-search, .multiselect-options, .multiselect-no-results {
        background-color: var(--color-bg-form-control) !important;
        border-color: var(--color-border-form-control) !important;
        color: var(--color-body-content) !important;
    }

    .multiselect-tags {
        max-height: 224px !important;
        overflow: auto !important;
    }

    .multiselect-tag {
        background: var(--color-multiselect-tag) !important;
    }

    .multiselect-clear {
        display: none !important;
    }

    .multiselect.is-active {
        box-shadow: var(--box-shadow-multiselect-wrapper) !important;
    }

    .multiselect-dropdown {
        border-color: var(--color-bg-form-control) !important;
    }

    .btn-primary {
        background-color: var(--color-bg-btn-primary) !important;
        border-color: var(--color-bg-btn-primary) !important;
        color: var(--color-btn-primary) !important;
    }

    .btn-outline-primary {
        background-color: transparent !important;
        border-color: var(--color-border-btn-outline-primary) !important;
        color: var(--color-btn-outline-primary) !important;
    }

    .btn-danger {
        background-color: var(--color-bg-btn-danger) !important;
        border-color: var(--color-bg-btn-danger) !important;
    }

    .btn-success {
        background-color: var(--color-bg-btn-success) !important;
        border-color: var(--color-bg-btn-success) !important;
    }

    .btn-secondary {
        background-color: var(--color-bg-btn-secondary) !important;
        border-color: var(--color-bg-btn-secondary) !important;
    }

    .btn-history {
        background-color: var(--color-bg-sidebar-content) !important;
        border-color: var(--color-bg-sidebar-content) !important;
    }

    .fas.fa-trash.fa-list-doc {
        color: var(--color-fa-list-doc) !important;
    }

    .breadcrumb > li > a {
        color: var(--color-body-content) !important;
        text-decoration: underline !important;
    }

    .page-item.active .page-link {
        z-index: 0 !important;
    }

    @media (min-width: 768px) {
        main > .container-fluid {
            padding: 22px 15px;
        }
    }

    main > .container {
        padding: 60px 15px 0;
    }

    .h5-custom-modal {
        color: var(--color-h5-custom-modal) !important;
    }

    .modal-content {
        background-color: var(--color-bg-body-content) !important;
    }

        .modal-content > .modal-header {
            border-bottom: var(--color-border-header-modal) !important;
        }

        .modal-content > .modal-body > div > .list-group > .list-group-item {
            background-color: var(--color-bg-form-control) !important;
            color: var(--color-body-content) !important;
        }

    .background-white {
        background-color: var(--color-bg-sidebar-content) !important;
    }

    .span-muted {
        color: var(--color-span-muted) !important;
    }

    .btn-custom-cancel {
        background-color: var(--color-bg-btn-cancel) !important;
        color: var(--color-btn-cancel) !important;
        height: 38px !important;
    }

    .btn-close {
        filter: var(--filter-btn-close) !important;
    }

    .bg-primary {
        background-color: var(--color-bg-btn-primary) !important;
        color: var(--color-btn-primary) !important;
    }

    .bg-custom-primary {
        background-color: var(--color-bg-btn-primary) !important;
        opacity: 0.6 !important;
        padding: 8px 12px !important;
    }

    .bg-custom-success {
        background-color: var(--color-bg-btn-success) !important;
        opacity: 0.6 !important;
        padding: 8px 12px !important;
    }

    .badge.rounded-pill.bg-danger.badge-custom {
        background-color: var(--color-bg-btn-danger) !important;
    }

    .toast {
        background-color: var(--color-bg-toast) !important;
    }

    .toast-header {
        background-color: var(--color-bg-toast-content) !important;
        color: var(--color-toast-content) !important;
        border-bottom: var(--color-border-toast) !important;
    }

    .toast-body {
        background-color: var(--color-bg-toast-content) !important;
        color: var(--color-body-content) !important;
    }

        .toast-body.toast-primary {
            background-color: var(--color-bg-toast-content-primary) !important;
            color: var(--color-toast-content-primary) !important;
        }

        .toast-body.toast-success {
            background-color: var(--color-bg-toast-content-success) !important;
            color: var(--color-toast-content-success) !important;
            height: 3.2em;
        }

        .toast-body.toast-warning {
            background-color: var(--color-bg-toast-content-warning) !important;
            color: var(--color-toast-content-warning) !important;
        }

        .toast-body.toast-danger {
            background-color: var(--color-bg-toast-content-danger) !important;
            color: var(--color-toast-content-danger) !important;
        }

        .toast-body.toast-upload {
            background-color: var(--color-bg-toast-content-upload) !important;
            color: var(--color-body-content) !important;
        }

    .div-card > .card {
        background: var(--color-card-content) !important;
    }

        .div-card > .card > .card-body > .card-title {
            color: var(--color-card-title) !important;
        }

    .navbar-light {
        background-color: var(--color-bg-dropdown-menu) !important;
    }

    .text-black {
        color: var(--color-dropdown-menu) !important;
    }

    .btn-lang {
        background-color: var(--color-bg-dropdown-menu) !important;
        border-color: var(--color-bg-dropdown-menu) !important;
    }

    .icon-black {
        color: var(--color-dropdown-menu) !important;
    }

    .link-dark {
        color: var(--color-dropdown-menu) !important;
    }

    .btn-toggle {
        color: var(--color-dropdown-menu) !important;
    }

    .btn-toggle-nav a:hover,
    .btn-toggle-nav a:focus {
        background-color: var(--color-fade-buttons) !important;
    }

    .btn-toggle-nav > li > .active {
        background-color: var(--color-fade-buttons) !important;
    }

    .btn-toggle::before {
        -webkit-filter: var(--content-chevron) !important;
        filter: var(--content-chevron) !important;
    }

    .input-group-text {
        background-color: var(--color-bg-form-control) !important;
        border-color: var(--color-border-form-control) !important;
        color: var(--color-upload-icon) !important;
    }

    .page-item:not(:first-child) .page-link {
        margin-left: 4px !important;
    }

    .table-bordered-vertical tr {
        border-right: 2px solid var(--color-bg-table-outline) !important;
        border-left: 2px solid var(--color-bg-table-outline) !important;
    }

    tr:last-child {
        border-bottom: 2px solid var(--color-bg-table-outline) !important;
    }

    tr:first-child {
        background-color: var(--color-bg-table-tr-first) !important;
    }

    .custom-padding {
        padding-left: 0;
        padding-right: 0;
        height: 100%;
    }

    @media (min-width: 1025px) {

        .custom-padding {
            padding-left: 70px;
            padding-right: 70px;
        }
    }

    .d-inline-custom {
        display: inline !important;
    }

    @media (max-width: 767px) {
        .d-inline-custom {
            display: none !important;
        }
    }

    .button-nav {
        background-color: var(--color-bg-nav-button) !important;
        color: var(--color-icon-nav-button) !important;
    }

    .dropdown-menu-button {
        background-color: var(--color-bg-dropdown-menu) !important;
        min-width: 5rem !important;
    }

        .dropdown-menu-button > .remove-hover > .dropdown-item {
            background-color: var(--color-bg-dropdown-menu) !important;
            color: var(--color-dropdown-menu) !important;
        }

    .theme-icon {
    }

        .theme-icon:before {
            font-family: 'Font Awesome 5 Free';
            content: var(--icon-theme);
            font-style: initial;
            font-weight: 900;
        }

    .nav-buttons {
        display: flex;
        gap: 10px;
        margin-left: auto;
    }

    .custom-class-dropzone {
        display: none !important;
    }

    .custom-dropzone .dropzone__progress {
        display: none !important;
    }

    .custom-dropzone .dropzone__item-thumbnail {
        border: solid 1px #0073e6 !important;
    }

    .delete-custom {
        background: var(--color-bg-table-outline) !important;
        border-color: var(--color-bg-table-outline) !important;
        color: var(--color-body-content) !important;
        height: 30px !important;
    }

    .dropzone {
        border: 1px dashed !important;
        border-color: var(--color-border-form-control) !important;
        background-color: var(--color-bg-form-control) !important;
    }
        .dropzone .dz-message .dz-button {
            background: none;
            color: inherit;
            border: none;
            padding: 0;
            font: inherit;
            cursor: pointer;
            outline: inherit;
            font-size: 0 !important;
        }

    .dz-message::before {
        content: '\f093'; 
        font-family: 'Font Awesome 5 Free'; 
        font-weight: 900; 
        font-size: 30px; 
        color: var(--color-border-form-control) !important;
    }

    .dropzone .dz-preview.dz-file-preview .dz-image {
        border-radius: 20px;
        border: solid 1px;
        border-color: var(--color-border-form-control) !important;
        background: var(--color-bg-nav-button) !important;
        z-index: 0;
    }

    .dropzone .dz-preview .dz-progress {
        display: none !important;
    }

    .dz-remove::after {
        content: '\f00d'; 
        font-family: 'Font Awesome 5 Free'; 
        font-weight: 900; 
        font-size: 16px;
        color: var(--color-body-content) !important;
    }

    .dz-remove:hover::after {
        color: #d9534f;
    }

    .dz-remove {
        font-size: 0 !important;
        text-decoration: none !important;
        top: -.825rem !important;
        right: -.825rem !important;
        text-align: unset !important;
        position: absolute !important;
        color: var(--color-body-content) !important;
    }

    .dropzone .dz-preview .dz-details .dz-size span {
        background: var(--color-bg-nav-button) !important;
        color: var(--color-body-content) !important;
        z-index: 0;
    }

    .dropzone .dz-preview .dz-details .dz-filename span {
        background: var(--color-bg-nav-button) !important;
        color: var(--color-body-content) !important;
    }

    .dropzone .dz-preview .dz-details .dz-filename:not(:hover) {
        color: var(--color-body-content) !important;
    }
    
    dropzone .dz-preview .dz-details {
        z-index: 0;
    }

    .margin-label {
        margin-left: 5px;
        margin-right: 5px;
        text-decoration: none;
        color: var(--color-body-content) !important;
    }

    .showContent {
        margin-left: 2%;
    }
</style>