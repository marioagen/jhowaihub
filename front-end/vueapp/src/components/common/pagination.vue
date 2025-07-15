<template>
    <div class="d-inline-block" style="margin-left: 1%">
        <div class="d-inline-custom">
            <a>{{ (paginationData.currentPage - 1) * 10 + (dataList.length > 0 ? 1 : 0) }} -</a>
            <a>{{ (paginationData.currentPage - 1) * 10 + dataList.length }}</a>
            <a class="margin-label">{{ $t('labelShowingToTotal') }}</a>
            <a>{{ paginationData.rowCount }}</a>
        </div>
    </div>
</template>
<script>
    export default {
        name: 'Pagination',
        props: {
            paginationData: {
                required: true,
                type: Object,
                default: {},
            },
            dataList: {
                required: true,
                type: Array,
                default: {},
            },
        },
        data() {
            return {
                title: 'Component pagination',
            }
        },
        components: {},
        watch: {},
        methods: {},
        computed: {},
        created() {},
        mounted() {},
        unmounted() {},
    }
</script>
