<template>
    <div class="dropdown position-static">
        <a 
            class="btn p-0 border-0"
            role="button"
            data-bs-toggle="dropdown"
            aria-expanded="false"
        >
            <slot name="header">
                <LucideIcon icon="Ellipsis" />
            </slot>
        </a>
        <ul class="dropdown-menu dropdown-menu-end">
            <slot />
        </ul>
    </div>
</template>

<script>
    export default {
        name: "DropdownComponent",
    };
</script>