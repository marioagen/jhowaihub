import { createRouter, createWebHashHistory } from "vue-router";
import LogoutIndex from "@/components/pages/logout";
import DocumentUpload from "@/components/pages/document/upload";
import DocumentList from "@/components/pages/document/list";
import NormalizeIndex from "@/components/pages/normalize/loading";
import AnalyzerIndex from "@/components/pages/analyzer";

import TypesPage from "@/pages/types.vue";
import QuizzesPage from "@/pages/quizzes/index.vue";
import NewQuizz from "@/pages/quizzes/newQuizz.vue";
import EditQuizz from "@/pages/quizzes/editQuizz.vue";
import QuestionsPage from "@/pages/questions.vue";
import UserManagePage from "@/pages/user-manager.vue";
import LoginIndex from "@/pages/login.vue";
import UnauthorizedPage from "@/pages/unauthorized.vue";

import { hasPermission } from "@/utils/permissions";
function authenticate(to, from, next) {
    const userStr = window.localStorage.getItem("project");
    const user = userStr ? JSON.parse(userStr) : null;
    if (!user) {
        return next({ path: "/" });
    }

    if (user.isLogged !== true) {
        return next({ path: "/" });
    }

    if (!hasPermission(to)) {
        return next({ path: "/unauthorized" });
    }

    return next();
}

const routes = [
    {
        path: "/",
        name: "Login",
        component: LoginIndex,
        meta: { 
            layout: "auth",
        },
    },
    {
        path: "/logout",
        name: "Logout",
        component: LogoutIndex,
        meta: {
            public: true
        }
    },
    {
        path: "/unauthorized",
        name: "Unauthorized",
        component: UnauthorizedPage,
        meta: { 
            layout: "auth",
        },
    },
    {
        path: "/document-upload",
        name: "DocumentUpload",
        component: DocumentUpload,
        meta: { 
            layout: "default" 
        },
    },
    {
        path: "/document-list",
        name: "DocumentList",
        component: DocumentList,
        meta: { 
            layout: "default",
            permission: ["documents_view"],
        },
        // beforeEnter: authenticate,
    },
    {
        path: "/types",
        name: "Type",
        component: TypesPage,
        meta: {
            layout: "default",
            permission: ["types_view"],
        },
        // beforeEnter: authenticate,
    },
    {
        path: "/questions",
        name: "Question",
        component: QuestionsPage,
        meta: { 
            layout: "default",
            permission: ["questions_view"],
        },
        // beforeEnter: authenticate,
    },
    {
        path: "/quizzes",
        name: "Quiz",
        component: QuizzesPage,
        meta: { 
            layout: "default" 
        },
    },
    {
        path: "/quizzes/new",
        name: "NewQuizz",
        component: NewQuizz,
        meta: { 
            layout: "default" 
        },
    },
    {
        path: "/quizzes/edit/:id",
        name: "EditQuizz",
        component: EditQuizz,
        meta: { 
            layout: "default" 
        },
    },
    {
        path: "/normalize/:id",
        name: "Normalize",
        component: NormalizeIndex,
        meta: { 
            layout: "default" 
        },
    },
    {
        path: "/analyzer/:id",
        name: "Analyzer",
        component: AnalyzerIndex,
        meta: { 
            layout: "default" 
        },
    },
    {
        path: "/manage-user",
        name: "UserManage",
        component: UserManagePage,
        meta: { 
            layout: "default" 
        },
    },
];

const router = createRouter({
    history: createWebHashHistory(),
    routes,
});

export default router;