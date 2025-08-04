import { createRouter, createWebHashHistory } from "vue-router";
import LoginIndex from "@/components/pages/login";
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

import { hasPermission } from "@/utils/permissions";

function authenticate(to, from, next) {
    var usuario = JSON.parse(window.localStorage.getItem("project"));
    if (usuario != null) {
        if (usuario.isLogged === true) {
            next();
        }
    } else
        next({
            path: "/",
        });
}

function requirePermission(permissionKey) {
    return (to, from, next) => {
        authenticate(to, from, () => {
            const permissionsList = permissionKey.split(',');
            let isAllowed = false;

            for (const permission of permissionsList) {
                if(hasPermission(permission)) {
                    isAllowed = true;
                    break;
                }
            }

            if (isAllowed) {
                next();
            } else {
                next({ path: '/unauthorized' });
            }
        });
    };
}

const routes = [
    {
        path: "/",
        name: "Login",
        component: LoginIndex,
        meta: { layout: "auth" },
    },
    {
        path: "/logout",
        name: "Logout",
        component: LogoutIndex,
    },
    {
        path: "/document-upload",
        name: "DocumentUpload",
        component: DocumentUpload,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Documents:Upload"),
    },
    {
        path: "/document-list",
        name: "DocumentList",
        component: DocumentList,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Documents:View"),
    },
    {
        path: "/types",
        name: "Type",
        component: TypesPage,
        meta: {
            layout: "default",
        },
        beforeEnter: requirePermission("Types:View"),
    },
    {
        path: "/questions",
        name: "Question",
        component: QuestionsPage,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Questions:View"),
    },
    {
        path: "/quizzes",
        name: "Quiz",
        component: QuizzesPage,
        meta: { layout: "default" },
        beforeEnter: authenticate,
    },
    {
        path: "/quizzes/new",
        name: "NewQuizz",
        component: NewQuizz,
        meta: { layout: "default" },
        beforeEnter: authenticate,
    },
    {
        path: "/quizzes/edit/:id",
        name: "EditQuizz",
        component: EditQuizz,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Documents:View"),
    },
    {
        path: "/normalize/:id",
        name: "Normalize",
        component: NormalizeIndex,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Documents:View"),
    },
    {
        path: "/analyzer/:id",
        name: "Analyzer",
        component: AnalyzerIndex,
        meta: { layout: "default" },
        beforeEnter: requirePermission("Documents:View"),
    },
    {
        path: "/manage-user",
        name: "UserManage",
        component: UserManagePage,
        meta: { layout: "default" },
        beforeEnter: requirePermission("ManageUsers:View"),
    },
];

const router = createRouter({
    history: createWebHashHistory(),
    routes,
});

export default router;
